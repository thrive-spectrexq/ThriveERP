# ThriveERP — Architecture

This document describes the internal architecture of ThriveERP: how the layers are organized, how a request flows through the system, and the reasoning behind the major design decisions. It's meant to be read before writing code, and kept up to date as the design evolves.

---

## Table of Contents

* [Guiding Principles](#guiding-principles)
* [Layer Overview](#layer-overview)
* [Dependency Rule](#dependency-rule)
* [Domain Layer](#domain-layer)
* [Application Layer](#application-layer)
* [Infrastructure Layer](#infrastructure-layer)
* [Desktop Layer (Avalonia UI)](#desktop-layer-avalonia-ui)
* [Request Flow — Worked Example](#request-flow--worked-example)
* [Cross-Cutting Concerns](#cross-cutting-concerns)
* [Composition Root & Dependency Injection](#composition-root--dependency-injection)
* [Cross-Platform Considerations](#cross-platform-considerations)
* [Design Decisions & Trade-offs](#design-decisions--trade-offs)
* [Future Extensibility](#future-extensibility)

---

## Guiding Principles

ThriveERP follows **Clean Architecture** as a **modular monolith**:

1. **Business logic is independent of frameworks.** The Domain layer has zero knowledge of EF Core, Avalonia, or any external library.
2. **Dependencies point inward.** Outer layers depend on inner layers, never the reverse. Inner layers define interfaces; outer layers implement them.
3. **The UI is a thin shell.** Avalonia ViewModels orchestrate calls into the Application layer; they contain no business rules.
4. **Testability first.** Because business logic has no infrastructure dependencies, the Domain and Application layers are unit-testable without a database, a UI, or mocks of either.
5. **One codebase, one process, multiple platforms.** ThriveERP is a single deployable application (a monolith), not microservices — but it's still cross-platform by virtue of Avalonia and .NET.

---

## Layer Overview

```
┌──────────────────────────────────────────────────────┐
│  ThriveERP.Desktop (Avalonia UI)                     │
│  Views (.axaml) · ViewModels · Converters · Behaviors │
│  Runs on Windows · macOS · Linux                      │
└───────────────────────┬────────────────────────────────┘
                         │ sends Commands/Queries via MediatR
┌───────────────────────▼────────────────────────────────┐
│  ThriveERP.Application                                │
│  Command/Query Handlers · DTOs · Validators            │
│  Interfaces (IRepository, IEmailService, IExportService)│
└───────────────────────┬────────────────────────────────┘
                         │ operates on
┌───────────────────────▼────────────────────────────────┐
│  ThriveERP.Domain                                     │
│  Entities · Value Objects · Domain Events · Business    │
│  Rules · Domain Exceptions                              │
│  (no external dependencies whatsoever)                  │
└───────────────────────┬────────────────────────────────┘
                         │ implemented by
┌───────────────────────▼────────────────────────────────┐
│  ThriveERP.Infrastructure                             │
│  EF Core DbContext · Repositories · SQLite · Serilog     │
│  sinks · QuestPDF/ClosedXML exporters · File storage     │
└──────────────────────────────────────────────────────┘
```

Each box is a separate .NET project (`.csproj`) with its own project references, enforcing the dependency rule at compile time rather than by convention alone.

---

## Dependency Rule

| Layer | May reference | Must never reference |
|---|---|---|
| Domain | (nothing — no project references at all) | Application, Infrastructure, Desktop, EF Core, Avalonia |
| Application | Domain | Infrastructure, Desktop, EF Core, Avalonia |
| Infrastructure | Domain, Application | Desktop, Avalonia |
| Desktop | Application (for MediatR requests/DTOs), Domain (for enums/display logic only where unavoidable) | Infrastructure directly — the Desktop project should never new up a `DbContext` or repository itself |

This is enforced two ways:
- **Project references** — the `.csproj` files simply don't reference the forbidden projects, so a violation is a compile error, not a code-review nitpick.
- **Architecture tests** (recommended, see [Testing Strategy](../../README.md#testing-strategy)) — a small xUnit project using `NetArchTest.Rules` or similar that asserts the dependency graph at build time, catching accidental violations (e.g. a Domain entity that sneaks in a `using Microsoft.EntityFrameworkCore`) before they reach review.

---

## Domain Layer

**Project:** `ThriveERP.Domain`
**Depends on:** nothing

Contains the core business model, expressed in plain C# with no external packages:

- **Entities** — `Product`, `Customer`, `Supplier`, `SalesOrder`, `Invoice`, `StockMovement`, `Employee`, etc. Entities own their invariants (e.g. a `StockLevel` entity refuses to go negative; a `SalesOrder` refuses to transition from `Draft` to `Paid` without going through `Invoiced` first).
- **Value Objects** — immutable types like `Money`, `Barcode`, `Address` that wrap primitive values with validation and equality semantics, so a stray `decimal` can't be mistaken for a `decimal` that represents money in the wrong currency.
- **Domain Events** — e.g. `StockLevelBelowThresholdEvent`, `SalesOrderPaidEvent` — raised by entities when something business-relevant happens, and dispatched by the Application layer after a successful save (see [Cross-Cutting Concerns](#cross-cutting-concerns)).
- **Domain Exceptions** — e.g. `InsufficientStockException`, `InvalidOrderStateTransitionException` — thrown by entities themselves, not by handlers, so the rule and its enforcement live in the same place.
- **Enums** — `OrderStatus`, `PaymentMethod`, `UserRole`, etc.

**Rule of thumb:** if a class needs `using Microsoft.EntityFrameworkCore` or `using Avalonia`, it does not belong in this project.

---

## Application Layer

**Project:** `ThriveERP.Application`
**Depends on:** Domain

This is where use cases live, implemented as **CQRS** via MediatR:

- **Commands** — write operations, e.g. `CreateSalesOrderCommand`, `AdjustStockCommand`, `VoidInvoiceCommand`. Each has a matching `CommandHandler` that loads entities via a repository interface, invokes domain behavior, and persists via `IUnitOfWork`.
- **Queries** — read operations, e.g. `GetLowStockProductsQuery`, `GetCustomerStatementQuery`. Query handlers can bypass the Domain model and query directly into lightweight DTOs for performance, since reads don't need to enforce write-side invariants.
- **DTOs** — data shapes exposed to the Desktop layer, mapped from entities via AutoMapper. The Desktop layer never receives raw Domain entities directly, which keeps the UI decoupled from Domain-model refactors.
- **Validators** — FluentValidation validators registered as a MediatR pipeline behavior, so every command is validated before its handler runs. This is the first validation gate; the second is Domain invariants enforced inside entities.
- **Interfaces for outward-facing concerns** — `IProductRepository`, `ISalesOrderRepository`, `IUnitOfWork`, `IPdfExportService`, `IExcelExportService`, `IEmailService`. These are defined here and implemented in Infrastructure — this is the **Dependency Inversion** that keeps Application decoupled from EF Core.

```
Command/Query flow:

Desktop ViewModel
   │  mediator.Send(new CreateSalesOrderCommand { ... })
   ▼
MediatR Pipeline
   │  ValidationBehavior<TRequest,TResponse>   (FluentValidation)
   │  LoggingBehavior<TRequest,TResponse>      (Serilog)
   ▼
CreateSalesOrderCommandHandler
   │  loads entities via ISalesOrderRepository, IProductRepository
   │  calls domain methods (order.AddItem(...), order.Submit())
   │  unitOfWork.SaveChangesAsync()
   ▼
Domain entities raise events → dispatched after save
   ▼
Handler returns a DTO → mapped via AutoMapper → back to ViewModel
```

---

## Infrastructure Layer

**Project:** `ThriveERP.Infrastructure`
**Depends on:** Domain, Application

Implements every interface the Application layer defines, plus anything that talks to the outside world:

- **EF Core `ThriveErpDbContext`** — entity configurations (via `IEntityTypeConfiguration<T>`, one file per entity, kept out of the Domain project), migrations, SQLite provider setup.
- **Repositories** — concrete implementations of `IProductRepository`, `ISalesOrderRepository`, etc., wrapping EF Core `DbSet<T>` queries.
- **Unit of Work** — wraps `DbContext.SaveChangesAsync()`, and is the point where domain events get dispatched post-commit (see [Cross-Cutting Concerns](#cross-cutting-concerns)).
- **Exporters** — `QuestPdfExportService`, `ClosedXmlExportService` implementing `IPdfExportService` / `IExcelExportService`.
- **Logging sinks** — Serilog configuration (file sink under `logs/`, console sink for dev).
- **File/backup storage** — local filesystem access for the SQLite backup routine described in the [README](../../README.md#backup-and-recovery).

Because everything here sits behind an Application-layer interface, swapping SQLite for PostgreSQL later — or adding a cloud sync provider in the v3.0 roadmap — means writing a new Infrastructure implementation, not touching Application or Domain code.

---

## Desktop Layer (Avalonia UI)

**Project:** `ThriveERP.Desktop`
**Depends on:** Application, Domain (display-only usage)

- **Views** (`.axaml`) — declarative UI markup, Avalonia's XAML dialect. Shared across Windows/macOS/Linux with no platform-specific branching in normal cases.
- **ViewModels** — built with `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`). A ViewModel's only job is to hold UI state and dispatch MediatR commands/queries — no EF Core, no business rules.
- **Converters / Behaviors** — Avalonia value converters and attached behaviors for UI-only concerns (formatting, input masks, drag-and-drop for the POS screen).
- **App composition** (`App.axaml.cs`) — configures the DI container and MediatR/EF Core registration at startup (see [Composition Root](#composition-root--dependency-injection)).

**Rule of thumb:** a ViewModel should read like a script of *"gather input → send a Command/Query → bind the result"*. If a ViewModel contains an `if` statement enforcing a business rule (e.g. "don't allow checkout if stock is zero"), that rule has leaked out of the Domain layer and needs to move back.

---

## Request Flow — Worked Example

**Scenario:** a cashier finalizes a POS sale.

1. **View** — `PosView.axaml` binds the "Complete Sale" button to `PosViewModel.CompleteSaleCommand`.
2. **ViewModel** — `CompleteSaleCommand` builds a `CreateSalesOrderCommand` (Application-layer request) from the current cart state and calls `_mediator.Send(command)`.
3. **Validation pipeline behavior** — a `CreateSalesOrderCommandValidator` (FluentValidation) checks required fields (customer, at least one line item, non-negative quantities) before the handler runs.
4. **Handler** — `CreateSalesOrderCommandHandler`:
   - loads the relevant `Product` entities via `IProductRepository`
   - constructs a `SalesOrder` domain entity and calls `order.AddItem(product, quantity)` for each cart line — stock-availability checks happen *inside* `AddItem`, raising `InsufficientStockException` if needed
   - calls `order.Submit()`, which raises a `SalesOrderSubmittedEvent`
   - persists via `IUnitOfWork.SaveChangesAsync()`
5. **Post-save** — the unit of work dispatches `SalesOrderSubmittedEvent` to any registered domain event handlers (e.g. one that decrements `StockLevel`, another that queues a receipt print job).
6. **Response** — the handler maps the saved `SalesOrder` to a `SalesOrderDto` via AutoMapper and returns it.
7. **ViewModel** — receives the DTO, updates bound properties (`LastSaleTotal`, `ReceiptNumber`), and triggers the receipt print/export flow.

At no point does the ViewModel touch EF Core, and at no point does the Domain layer know a UI exists.

---

## Cross-Cutting Concerns

| Concern | Where it lives | Mechanism |
|---|---|---|
| Validation | Application layer | FluentValidation validators run as a MediatR pipeline behavior, ahead of every handler |
| Logging | Infrastructure (sinks), Application (pipeline behavior) | Serilog; a `LoggingBehavior<TRequest,TResponse>` logs every command/query with duration and outcome |
| Domain events | Domain (raised), Infrastructure (dispatched) | Entities collect events in an internal list; `UnitOfWork.SaveChangesAsync()` dispatches them after a successful commit, so handlers never react to a change that got rolled back |
| Mapping | Application | AutoMapper profiles, one per module, kept next to the DTOs they map |
| Authentication/Authorization | Application (policy checks in handlers or a pipeline behavior), Infrastructure (BCrypt hashing) | A `CurrentUserContext` service (Infrastructure-implemented, Application-defined interface) supplies the active user's roles/permissions to handlers |
| Auditing | Domain events → Infrastructure | Sensitive actions (price override, sale void, user role change) raise a domain event consumed by an `AuditLogWriter` |

---

## Composition Root & Dependency Injection

`App.axaml.cs` in the Desktop project is the composition root — the one place that knows about every layer at once:

```csharp
var services = new ServiceCollection();

services.AddDomainServices();          // Domain — usually just a marker extension, no DI needed
services.AddApplicationServices();     // registers MediatR, FluentValidation, AutoMapper profiles
services.AddInfrastructureServices(configuration); // DbContext, repositories, exporters, Serilog

var provider = services.BuildServiceProvider();
```

Each layer exposes an `IServiceCollection` extension method (`AddApplicationServices`, `AddInfrastructureServices`) so the Desktop project doesn't need to know *how* each layer wires itself up — only that it needs to be wired.

---

## Cross-Platform Considerations

Because the UI runs on Avalonia rather than a Windows-only framework, a few things are called out explicitly so they don't get assumed away during development:

- **File paths** — use `Path.Combine` and `Environment.GetFolderPath` rather than hard-coded `\`-style paths, so the SQLite database and log/backup locations resolve correctly on macOS and Linux.
- **SQLite file locations** — default to the platform-appropriate app-data directory (`%AppData%` on Windows, `~/Library/Application Support` on macOS, `~/.local/share` on Linux) rather than the executable's working directory.
- **Printing** — receipt/label printing goes through Avalonia's platform-abstracted printing APIs (or a dedicated printing library) rather than a Windows-only print API, since POS receipt printing is a core feature.
- **Packaging** — each platform gets its own installer/package format (MSI or MSIX for Windows, `.dmg`/notarized app bundle for macOS, AppImage or `.deb` for Linux), built from the same Desktop project via platform-specific `dotnet publish` profiles.
- **Fonts/DPI** — Avalonia handles most DPI-scaling differences automatically, but custom-drawn controls (e.g. a barcode-scanner input overlay) should be tested on all three platforms before release, not just Windows.

---

## Design Decisions & Trade-offs

| Decision | Reasoning | Trade-off accepted |
|---|---|---|
| Modular monolith over microservices | Single-business, single-install target; microservices would add operational complexity with no benefit for an offline-first desktop app | Less independent scalability — acceptable, since ThriveERP isn't a distributed system |
| CQRS via MediatR | Keeps the Desktop layer decoupled from handler implementation details; makes cross-cutting concerns (validation, logging) trivial to add as pipeline behaviors | Slightly more ceremony than calling a service method directly — one Command/Handler/Validator per use case |
| SQLite as the only supported database for v1.0 | Zero-config, single-file, matches the offline-first pitch | No built-in multi-user write concurrency — acceptable for the single-workstation v1.0 target; PostgreSQL support is on the v3.0 roadmap for multi-branch scenarios |
| Avalonia UI over WPF | Cross-platform (Windows/macOS/Linux) without a rewrite; keeps XAML/MVVM patterns familiar to .NET developers | Smaller component ecosystem than WPF's; some third-party WPF controls have no Avalonia equivalent yet and may need custom implementations |
| Domain entities enforce their own invariants (rich domain model) | Business rules can't be bypassed by a handler that forgets to check them | More upfront design work per entity than an anemic-model + service-layer approach |

---

## Future Extensibility

The architecture is deliberately set up so the v3.0 roadmap items don't require restructuring:

- **Cloud sync (opt-in)** — a new Infrastructure implementation of a sync service, triggered by domain events, without touching Domain or Application code.
- **Multi-branch support** — mostly a Domain-layer concern (a `Branch`/`Warehouse` scoping concept already exists via `Warehouses`/`StockLevels`); Application-layer queries would need branch-scoping added to their filters.
- **Companion mobile app** — since Application-layer use cases are already expressed as Commands/Queries with DTOs, they map naturally onto a future Web API project that a mobile client could call, reusing Domain and Application almost unchanged.
- **PostgreSQL support** — swap the Infrastructure-layer `DbContext` provider and repository implementations; Domain and Application are untouched because they only depend on repository *interfaces*.
