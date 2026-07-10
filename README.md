# ThriveERP

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/UI-Avalonia-6E40C9)](https://avaloniaui.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](#)
[![Status](https://img.shields.io/badge/status-pre--release-orange)](#roadmap)

**ThriveERP** is an offline-first Enterprise Resource Planning (ERP) desktop application for small and medium-sized businesses.

Built with **C#, .NET 9, Avalonia UI, and SQLite**, ThriveERP is a fast, reliable, single-install solution for managing inventory, sales, purchases, customers, suppliers, accounting, employees, and reporting — with zero cloud dependency, zero recurring cost, and native support for **Windows, macOS, and Linux**.

---

## Table of Contents

* [Why ThriveERP](#why-thriveerp)
* [Key Features](#key-features)
* [Target Users](#target-users)
* [Architecture](#architecture)
* [Technology Stack](#technology-stack)
* [Project Structure](#project-structure)
* [Modules](#modules)
* [Database Design](#database-design)
* [Security](#security)
* [Testing Strategy](#testing-strategy)
* [Installation](#installation)
* [Development Setup](#development-setup)
* [Configuration](#configuration)
* [Running ThriveERP](#running-thriveerp)
* [Backup and Recovery](#backup-and-recovery)
* [CI/CD](#cicd)
* [Roadmap](#roadmap)
* [Contributing](#contributing)
* [License](#license)

---

## Why ThriveERP

| Problem with typical ERPs | ThriveERP's approach |
|---|---|
| Expensive monthly subscriptions | One-time install, no recurring fees |
| Requires constant internet access | Fully offline-first, local SQLite storage |
| Complex, multi-week setup | Single installer, database created on first run |
| Vendor lock-in on your data | Your data lives in a portable `.db` file you own |
| Bloated feature sets you'll never use | Modular — enable only the modules your business needs |
| Locked to a single OS | Avalonia UI runs natively on Windows, macOS, and Linux |

---

## Key Features

Manage daily operations from one application: **Customers · Suppliers · Products · Inventory · Sales · Purchases · Invoices · Payments · Expenses · Employees · Reports**

### Point of Sale (POS)
Product search, barcode scanning, quick checkout, receipt printing, multiple payment methods, discounts, returns, customer purchase history, daily sales summaries.

### Inventory Management
Product catalog, categories, brands, stock quantity and adjustments, stock movement history, low-stock alerts, multi-warehouse support, batch tracking, expiry date tracking, inventory valuation (FIFO/weighted average).

### Customer Management
Customer profiles, contact info, purchase history, credit accounts, payment tracking, customer statements.

### Supplier Management
Supplier profiles, purchase history, supplier balances, product-supplier mapping, payment records.

### Sales Management
Sales orders → invoices → payments, returns, discounts, full sales history.

### Purchase Management
Purchase orders, goods receiving, supplier invoices, purchase history, cost tracking.

### Accounting
Income and expense tracking, cash management, customer/supplier balances, profit & loss reports, financial summaries.

### Employee Management
Employee records, user accounts, roles, permissions, activity/audit tracking.

### Reporting
Daily/monthly sales, inventory, purchases, expenses, customers, suppliers, profit, and product performance reports — exportable to **PDF, Excel, and CSV**.

---

## Target Users

| Business Type | Notable Needs Covered |
|---|---|
| Retail shops (grocery, electronics, clothing, general stores) | POS, inventory, barcode scanning |
| Pharmacies | Expiry tracking, batch management |
| Restaurants | Sales tracking, stock control, expense management |
| Small manufacturers | Raw materials, production records, purchase tracking |

---

## Architecture

ThriveERP follows **Clean Architecture** (a modular monolith) so business logic stays independent of the UI and database, making it testable and easier to extend toward multi-branch/cloud sync later.

```
┌────────────────────────────────────────────┐
│      ThriveERP.Desktop (Avalonia UI)       │   Presentation — Views, ViewModels (MVVM)
│   Runs on Windows · macOS · Linux          │
└───────────────────┬────────────────────────┘
                     │
┌───────────────────▼──────────────────────────┐
│         ThriveERP.Application                │  Use cases, CQRS commands/queries,
│  (MediatR handlers, DTOs, validators)        │  business workflows, FluentValidation
└───────────────────┬──────────────────────────┘
                     │
┌───────────────────▼──────────────────────────┐
│           ThriveERP.Domain                   │  Entities, value objects, domain
│  (no external dependencies)                  │  events, business rules
└───────────────────┬──────────────────────────┘
                     │
┌───────────────────▼──────────────────────────┐
│        ThriveERP.Infrastructure              │  EF Core, SQLite, file storage,
│  (implements Application interfaces)         │  logging, PDF/Excel export
└───────────────────┬──────────────────────────┘
                     │
┌───────────────────▼──────────────────────────┐
│              SQLite Database                 │
│              ThriveERP.db                    │
└────────────────────────────────────────────  ┘
```

**Why this matters for you:** dependencies point inward (Desktop → Application → Domain), so the Domain layer never references EF Core or Avalonia. That means you can swap SQLite for PostgreSQL later, or add a web front-end, without touching business logic. Because the UI layer is Avalonia instead of a Windows-only framework, the same Desktop project also compiles and runs on macOS and Linux with no architectural changes.

---

## Technology Stack

### Application

| Technology | Purpose |
|---|---|
| C# | Main programming language |
| .NET 9 | Application framework |
| Avalonia UI | Cross-platform desktop UI (Windows, macOS, Linux) |
| MVVM (CommunityToolkit.Mvvm) | UI architecture, reduces boilerplate |
| MediatR | CQRS — decouples UI from business logic |
| AutoMapper | Entity ↔ DTO mapping |

### Database

| Technology | Purpose |
|---|---|
| SQLite | Local embedded database |
| Entity Framework Core 9 | ORM / database access |
| EF Core Migrations | Schema versioning |

### Additional Libraries

| Library | Purpose |
|---|---|
| Serilog | Structured application logging |
| QuestPDF | PDF report generation |
| ClosedXML | Excel export |
| FluentValidation | Input/data validation |
| BCrypt.Net | Password hashing |
| xUnit + FluentAssertions | Automated testing |
| Moq | Mocking in unit tests |

---

## Project Structure

```
ThriveERP/
├── src/
│   ├── ThriveERP.Domain/            # Entities, enums, domain rules — no dependencies
│   ├── ThriveERP.Application/       # Use cases, CQRS handlers, DTOs, interfaces
│   ├── ThriveERP.Infrastructure/    # EF Core, repositories, external services
│   └── ThriveERP.Desktop/           # Avalonia app, Views, ViewModels
├── database/
│   ├── migrations/
│   └── seed-data/
├── docs/
│   ├── architecture/
│   ├── assets/
│   └── database-schema.md
├── tests/
│   ├── ThriveERP.Domain.Tests/
│   ├── ThriveERP.Application.Tests/
│   └── ThriveERP.Infrastructure.Tests/
├── .github/workflows/               # CI pipelines
├── LICENSE
└── README.md
```

---

## Modules

| Module | Responsibility |
|---|---|
| **Core** | App settings, user accounts, authentication, permissions, audit logs |
| **Inventory** | Products, categories, stock, warehouses, stock movements |
| **Sales** | POS, sales orders, invoices, payments, returns |
| **Purchase** | Suppliers, purchase orders, goods receiving |
| **Accounting** | Transactions, expenses, financial reports |
| **Reporting** | Report generation, data export, business summaries |

---

## Database Design

Core entity groups (see `docs/database-schema.md` for full column-level schema once modeled):

```
Identity:      Users, Roles, Permissions, AuditLogs
Parties:       Customers, Suppliers
Catalog:       Products, Categories, Brands
Inventory:     Warehouses, StockLevels, StockMovements, Batches
Sales:         SalesOrders, SaleItems, Invoices, Payments, Returns
Purchasing:    PurchaseOrders, PurchaseItems, SupplierInvoices
Accounting:    Transactions, Expenses, Accounts
HR:            Employees
```

**Before writing code**, model this as an ER diagram and define:
- Primary/foreign key relationships and cascade rules
- Which fields are nullable vs required
- Indexes on frequently-queried columns (e.g. `Products.Barcode`, `Sales.Date`)
- Soft-delete strategy (recommended: `IsDeleted` flag, not hard deletes, for audit trail integrity)

---

## Security

* User authentication with hashed passwords (BCrypt, never plain-text or reversible encryption)
* Role-based access control (RBAC) with granular permissions per module
* Activity/audit logs for sensitive actions (sales voids, price overrides, user changes)
* Input validation at the Application layer (FluentValidation) — never trust UI-layer validation alone
* Parameterized queries via EF Core (protects against SQL injection by default)
* **Recommended addition:** encrypt the SQLite file at rest using SQLCipher, since a stolen laptop currently means a fully readable database

---

## Testing Strategy

A gap in most solo ERP projects — plan for it from day one, not after v1.0:

| Layer | Test Type | Tooling |
|---|---|---|
| Domain | Unit tests on business rules (e.g. stock can't go negative) | xUnit |
| Application | Unit tests on CQRS handlers with mocked repositories | xUnit + Moq |
| Infrastructure | Integration tests against a real SQLite test database | xUnit + EF Core InMemory/SQLite |
| Desktop | Manual QA checklist per release (Windows/macOS/Linux); ViewModel unit tests where feasible | xUnit |

Target: business-critical modules (Sales, Inventory, Accounting) should have meaningful coverage before v1.0 ships, since these touch money and stock counts.

---

## Installation

### Requirements

* .NET 9 SDK
* Visual Studio 2026 or later, JetBrains Rider, or VS Code with the C# Dev Kit and Avalonia extension
* Windows 10/11, macOS 12+, or a modern Linux distribution (Avalonia UI is cross-platform, so ThriveERP is no longer tied to a single OS)

---

## Development Setup

```bash
git clone https://github.com/thrive-spectrexq/ThriveERP.git
cd ThriveERP
dotnet restore
dotnet ef database update --project src/ThriveERP.Infrastructure --startup-project src/ThriveERP.Desktop
dotnet build
```

---

## Configuration

Application settings live in `appsettings.json` (Desktop project):

```json
{
  "Database": {
    "ConnectionString": "Data Source=ThriveERP.db"
  },
  "Logging": {
    "MinimumLevel": "Information",
    "LogFilePath": "logs/thriveerp-.log"
  },
  "Backup": {
    "AutoBackupEnabled": true,
    "AutoBackupIntervalHours": 24,
    "BackupRetentionDays": 30
  }
}
```

---

## Running ThriveERP

From Visual Studio or Rider: **Start Debugging** (F5)

Or from the command line, on any supported platform:

```bash
dotnet run --project src/ThriveERP.Desktop
```

On first run, ThriveERP creates `ThriveERP.db` and seeds default roles (Admin, Manager, Cashier).

---

## Backup and Recovery

Because ThriveERP uses SQLite, backups are a single file copy.

```bash
# Backup (Windows)
copy ThriveERP.db backups/ThriveERP_2026-07-10.db

# Backup (macOS/Linux)
cp ThriveERP.db backups/ThriveERP_2026-07-10.db

# Restore (Windows)
copy backups/ThriveERP_2026-07-10.db ThriveERP.db

# Restore (macOS/Linux)
cp backups/ThriveERP_2026-07-10.db ThriveERP.db
```

**Planned improvements:**
- Automatic scheduled backups (configurable interval, see `appsettings.json` above)
- Encrypted backup files (AES-256)
- One-click restore from within the app, with a pre-restore safety snapshot

---

## CI/CD

Recommended before your first real release — cheap to set up now, painful to retrofit later:

* **GitHub Actions** workflow: `dotnet build` + `dotnet test` on every PR, run across a build matrix (Windows, macOS, Linux) to catch platform-specific Avalonia issues early
* Automated versioned releases (tag → build per-platform installers → attach as GitHub Release assets)
* Optional: code coverage reporting (Coverlet + Codecov) so test gaps are visible over time

---

## Roadmap

### Version 1.0 — Core Foundation
- [ ] User authentication & RBAC
- [ ] Product & inventory management
- [ ] Customer & supplier management
- [ ] Sales module (POS + invoicing)
- [ ] Basic reporting (PDF/Excel export)
- [ ] Avalonia UI shell with Windows/macOS/Linux builds verified

### Version 2.0 — Business Depth
- [ ] Accounting module
- [ ] Employee management
- [ ] Advanced reporting & dashboards
- [ ] Barcode label printing
- [ ] Multi-warehouse support
- [ ] Encrypted, scheduled backups

### Version 3.0 — Scale
- [ ] Cloud synchronization (optional, opt-in)
- [ ] Multi-branch support
- [ ] Companion mobile app
- [ ] Advanced analytics & forecasting

---

## Contributing

1. Fork the repository
2. Create a branch: `git checkout -b feature/new-feature`
3. Commit changes: `git commit -m "Add new feature"`
4. Push: `git push origin feature/new-feature`
5. Open a Pull Request

Please include tests for new business logic (see [Testing Strategy](#testing-strategy)) and keep Domain-layer code free of infrastructure dependencies.

---

## License

ThriveERP is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## Vision

ThriveERP aims to be a complete digital management solution for small businesses by combining simplicity, reliability, and solid cross-platform desktop architecture.

**Offline. Simple. Reliable. Cross-platform. Business-ready.**
