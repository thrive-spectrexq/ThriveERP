# ThriveERP — Database Schema

Column-level schema for the SQLite database (`ThriveERP.db`), managed via EF Core Migrations. This expands on the entity groups summarized in the [README](../README.md#database-design) and should stay in sync with the `IEntityTypeConfiguration<T>` classes in `ThriveERP.Infrastructure`.

---

## Table of Contents

* [Conventions](#conventions)
* [Identity](#identity)
* [Parties](#parties)
* [Catalog](#catalog)
* [Inventory](#inventory)
* [Sales](#sales)
* [Purchasing](#purchasing)
* [Accounting](#accounting)
* [HR](#hr)
* [Entity Relationship Overview](#entity-relationship-overview)
* [Indexing Strategy](#indexing-strategy)
* [Soft-Delete Strategy](#soft-delete-strategy)
* [Cascade Rules](#cascade-rules)
* [Migration Notes](#migration-notes)

---

## Conventions

- **Primary keys** — every table uses a `Guid Id` primary key (not an auto-increment `int`), so records can be created client-side before a save round-trip and remain stable across future sync/multi-branch scenarios.
- **Soft delete** — every table below includes an `IsDeleted` flag rather than supporting hard deletes; see [Soft-Delete Strategy](#soft-delete-strategy).
- **Auditing** — every table includes `CreatedAtUtc`, `CreatedByUserId`, `ModifiedAtUtc`, `ModifiedByUserId`. These are omitted from the column tables below to avoid repetition — assume they're present on every entity unless noted otherwise.
- **Money** — all monetary columns are `decimal(18,2)` and represent the entity's base currency; no multi-currency support in v1.0.
- **Required vs nullable** — marked per column; anything not marked nullable is `NOT NULL`.
- **Naming** — PascalCase table and column names, matching C# entity/property names (EF Core's default SQLite convention).

---

## Identity

### Users
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Username | string(50) | | Unique |
| PasswordHash | string(200) | | BCrypt hash, never plain-text |
| FullName | string(150) | | |
| Email | string(150) | ✓ | Unique when present |
| RoleId | Guid (FK → Roles.Id) | | |
| IsActive | bool | | Default `true`; disables login without deleting the account |
| LastLoginAtUtc | datetime | ✓ | |

### Roles
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Name | string(50) | | Unique — e.g. `Admin`, `Manager`, `Cashier` |
| Description | string(250) | ✓ | |

### Permissions
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Code | string(100) | | Unique — e.g. `sales.void`, `inventory.adjust` |
| Description | string(250) | ✓ | |

### RolePermissions (join table)
| Column | Type | Nullable | Notes |
|---|---|---|---|
| RoleId | Guid (FK → Roles.Id) | | Composite PK with PermissionId |
| PermissionId | Guid (FK → Permissions.Id) | | Composite PK with RoleId |

### AuditLogs
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| UserId | Guid (FK → Users.Id) | | Who performed the action |
| Action | string(100) | | e.g. `SaleVoided`, `PriceOverridden`, `UserRoleChanged` |
| EntityName | string(100) | | e.g. `SalesOrder` |
| EntityId | Guid | | Id of the affected record |
| Details | string(2000) | ✓ | JSON snapshot of what changed |
| OccurredAtUtc | datetime | | |

---

## Parties

### Customers
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Name | string(150) | | |
| Phone | string(30) | ✓ | |
| Email | string(150) | ✓ | |
| Address | string(300) | ✓ | |
| CreditLimit | decimal(18,2) | | Default `0` |
| CurrentBalance | decimal(18,2) | | Derived/maintained via Sales/Payments; not user-editable directly |
| IsActive | bool | | Default `true` |

### Suppliers
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Name | string(150) | | |
| Phone | string(30) | ✓ | |
| Email | string(150) | ✓ | |
| Address | string(300) | ✓ | |
| CurrentBalance | decimal(18,2) | | Amount owed to the supplier |
| IsActive | bool | | Default `true` |

---

## Catalog

### Categories
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Name | string(100) | | Unique |
| ParentCategoryId | Guid (FK → Categories.Id) | ✓ | Self-referencing, for subcategories |

### Brands
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Name | string(100) | | Unique |

### Products
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Sku | string(50) | | Unique |
| Barcode | string(50) | ✓ | Unique when present; indexed for POS scan lookups |
| Name | string(200) | | |
| Description | string(1000) | ✓ | |
| CategoryId | Guid (FK → Categories.Id) | ✓ | |
| BrandId | Guid (FK → Brands.Id) | ✓ | |
| CostPrice | decimal(18,2) | | |
| SellingPrice | decimal(18,2) | | |
| TrackBatches | bool | | Default `false` — enables batch/expiry tracking (pharmacies) |
| ReorderThreshold | int | | Default `0`; drives low-stock alerts |
| IsActive | bool | | Default `true` |

### ProductSuppliers (join table)
| Column | Type | Nullable | Notes |
|---|---|---|---|
| ProductId | Guid (FK → Products.Id) | | Composite PK with SupplierId |
| SupplierId | Guid (FK → Suppliers.Id) | | Composite PK with ProductId |
| SupplierSku | string(50) | ✓ | Supplier's own code for this product |
| LastPurchasePrice | decimal(18,2) | ✓ | |

---

## Inventory

### Warehouses
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Name | string(100) | | |
| Location | string(300) | ✓ | |
| IsDefault | bool | | Default `false`; exactly one warehouse should be default |

### StockLevels
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| ProductId | Guid (FK → Products.Id) | | |
| WarehouseId | Guid (FK → Warehouses.Id) | | |
| QuantityOnHand | decimal(18,3) | | Decimal to support fractional units (e.g. kg, liters); enforced non-negative at the Domain layer |
| — | | | Unique constraint on `(ProductId, WarehouseId)` |

### StockMovements
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| ProductId | Guid (FK → Products.Id) | | |
| WarehouseId | Guid (FK → Warehouses.Id) | | |
| BatchId | Guid (FK → Batches.Id) | ✓ | Set when `Product.TrackBatches` is true |
| MovementType | string(20) | | `Sale`, `Purchase`, `Adjustment`, `Transfer`, `Return` |
| QuantityChange | decimal(18,3) | | Positive for inbound, negative for outbound |
| ReferenceType | string(50) | ✓ | e.g. `SalesOrder`, `PurchaseOrder` — polymorphic reference |
| ReferenceId | Guid | ✓ | Id of the referenced Sales/Purchase record |
| Reason | string(250) | ✓ | Free-text, required by validation when `MovementType = Adjustment` |
| OccurredAtUtc | datetime | | |

### Batches
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| ProductId | Guid (FK → Products.Id) | | |
| BatchNumber | string(50) | | |
| ExpiryDate | date | ✓ | |
| ReceivedAtUtc | datetime | | |
| — | | | Unique constraint on `(ProductId, BatchNumber)` |

---

## Sales

### SalesOrders
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| OrderNumber | string(30) | | Unique, human-readable (e.g. `SO-2026-00042`) |
| CustomerId | Guid (FK → Customers.Id) | ✓ | Null for walk-in/anonymous POS sales |
| WarehouseId | Guid (FK → Warehouses.Id) | | Fulfilling location |
| Status | string(20) | | `Draft`, `Submitted`, `Invoiced`, `Paid`, `Voided` |
| Subtotal | decimal(18,2) | | |
| DiscountTotal | decimal(18,2) | | Default `0` |
| TaxTotal | decimal(18,2) | | Default `0` |
| GrandTotal | decimal(18,2) | | |
| OrderDate | datetime | | |

### SaleItems
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| SalesOrderId | Guid (FK → SalesOrders.Id) | | |
| ProductId | Guid (FK → Products.Id) | | |
| Quantity | decimal(18,3) | | |
| UnitPrice | decimal(18,2) | | Snapshot of `Product.SellingPrice` at time of sale |
| DiscountAmount | decimal(18,2) | | Default `0` |
| LineTotal | decimal(18,2) | | |

### Invoices
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| InvoiceNumber | string(30) | | Unique |
| SalesOrderId | Guid (FK → SalesOrders.Id) | | |
| IssuedAtUtc | datetime | | |
| DueDate | date | ✓ | Null for immediate-payment POS sales |
| AmountDue | decimal(18,2) | | |

### Payments
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| InvoiceId | Guid (FK → Invoices.Id) | ✓ | Null for supplier payments (see Purchasing) |
| CustomerId | Guid (FK → Customers.Id) | ✓ | |
| Amount | decimal(18,2) | | |
| Method | string(20) | | `Cash`, `Card`, `BankTransfer`, `MobileMoney`, `StoreCredit` |
| PaidAtUtc | datetime | | |
| ReceivedByUserId | Guid (FK → Users.Id) | | Cashier/user who took the payment |

### Returns
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| SalesOrderId | Guid (FK → SalesOrders.Id) | | |
| ProductId | Guid (FK → Products.Id) | | |
| Quantity | decimal(18,3) | | |
| RefundAmount | decimal(18,2) | | |
| Reason | string(250) | ✓ | |
| ProcessedByUserId | Guid (FK → Users.Id) | | |
| ProcessedAtUtc | datetime | | |

---

## Purchasing

### PurchaseOrders
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| OrderNumber | string(30) | | Unique |
| SupplierId | Guid (FK → Suppliers.Id) | | |
| WarehouseId | Guid (FK → Warehouses.Id) | | Receiving location |
| Status | string(20) | | `Draft`, `Ordered`, `PartiallyReceived`, `Received`, `Cancelled` |
| Subtotal | decimal(18,2) | | |
| TaxTotal | decimal(18,2) | | Default `0` |
| GrandTotal | decimal(18,2) | | |
| OrderDate | datetime | | |
| ExpectedDate | date | ✓ | |

### PurchaseItems
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| PurchaseOrderId | Guid (FK → PurchaseOrders.Id) | | |
| ProductId | Guid (FK → Products.Id) | | |
| QuantityOrdered | decimal(18,3) | | |
| QuantityReceived | decimal(18,3) | | Default `0`; updated as goods are received |
| UnitCost | decimal(18,2) | | |
| LineTotal | decimal(18,2) | | |

### SupplierInvoices
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| InvoiceNumber | string(30) | | Supplier's own invoice reference |
| PurchaseOrderId | Guid (FK → PurchaseOrders.Id) | | |
| AmountDue | decimal(18,2) | | |
| DueDate | date | ✓ | |
| ReceivedAtUtc | datetime | | |

---

## Accounting

### Accounts
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Name | string(100) | | e.g. `Cash Drawer`, `Bank — Main`, `Petty Cash` |
| Type | string(20) | | `Asset`, `Liability`, `Income`, `Expense`, `Equity` |
| Balance | decimal(18,2) | | Default `0`; maintained via Transactions |
| IsActive | bool | | Default `true` |

### Transactions
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| AccountId | Guid (FK → Accounts.Id) | | |
| Amount | decimal(18,2) | | Positive for credit, negative for debit (single-entry v1.0; see [Migration Notes](#migration-notes)) |
| Type | string(20) | | `Sale`, `Purchase`, `Expense`, `Refund`, `ManualAdjustment` |
| ReferenceType | string(50) | ✓ | e.g. `SalesOrder`, `Expense` — polymorphic reference |
| ReferenceId | Guid | ✓ | |
| Description | string(250) | ✓ | |
| OccurredAtUtc | datetime | | |

### Expenses
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| Category | string(100) | | e.g. `Rent`, `Utilities`, `Payroll` |
| Amount | decimal(18,2) | | |
| AccountId | Guid (FK → Accounts.Id) | | Which account the expense was paid from |
| Description | string(250) | ✓ | |
| ExpenseDate | date | | |
| RecordedByUserId | Guid (FK → Users.Id) | | |

---

## HR

### Employees
| Column | Type | Nullable | Notes |
|---|---|---|---|
| Id | Guid (PK) | | |
| UserId | Guid (FK → Users.Id) | ✓ | Null if the employee has no system login |
| FullName | string(150) | | |
| Position | string(100) | ✓ | |
| Phone | string(30) | ✓ | |
| Email | string(150) | ✓ | |
| HireDate | date | ✓ | |
| IsActive | bool | | Default `true` |

---

## Entity Relationship Overview

```
Users ──< RolePermissions >── Permissions
  │  \
  │   \── Roles
  │
  ├──< AuditLogs
  ├──< Payments (ReceivedByUserId)
  └──< Employees (UserId, optional)

Customers ──< SalesOrders ──< SaleItems >── Products
                  │               │
                  ├──< Invoices ──< Payments
                  └──< Returns >── Products

Suppliers ──< PurchaseOrders ──< PurchaseItems >── Products
                  │
                  └──< SupplierInvoices

Products >── Categories
Products >── Brands
Products ──< ProductSuppliers >── Suppliers
Products ──< StockLevels >── Warehouses
Products ──< StockMovements >── Warehouses
Products ──< Batches ──< StockMovements

Accounts ──< Transactions
Accounts ──< Expenses
```

`──<` denotes one-to-many; `>──` denotes many-to-one; `──< ... >──` denotes a many-to-many join table.

---

## Indexing Strategy

Beyond the primary keys (all indexed by default as `Guid` PKs), the following indexes are planned:

| Table | Index | Reason |
|---|---|---|
| Products | `Barcode` (unique, filtered where not null) | POS barcode-scan lookups need to be sub-millisecond |
| Products | `Sku` (unique) | Manual product lookup/search |
| SalesOrders | `OrderDate` | Date-range reports (daily/monthly sales) are a core Reporting feature |
| SalesOrders | `CustomerId` | Customer purchase history and statements |
| SalesOrders | `Status` | Filtering open/unpaid orders |
| PurchaseOrders | `SupplierId`, `OrderDate` | Supplier purchase history, purchasing reports |
| StockMovements | `(ProductId, WarehouseId, OccurredAtUtc)` | Stock movement history per product/location, chronological |
| StockLevels | `(ProductId, WarehouseId)` unique | Enforces one stock row per product-per-warehouse; also the natural lookup path |
| Transactions | `(AccountId, OccurredAtUtc)` | Account ledgers and financial reports |
| Users | `Username` (unique) | Login lookup |
| AuditLogs | `(EntityName, EntityId)` | Looking up the audit trail for a specific record |

---

## Soft-Delete Strategy

Every table carries an `IsDeleted` flag (default `false`) instead of supporting hard deletes, for two reasons:

1. **Audit trail integrity** — a deleted product, customer, or sales order shouldn't break historical reports that reference it (e.g. a report for last month shouldn't silently lose line items because the product was later removed from the catalog).
2. **Referential safety** — with soft delete, foreign keys always resolve; a hard delete on, say, a `Product` referenced by years of `SaleItems` would either cascade-delete sales history or require nullable FKs everywhere, both worse options.

Implementation:
- EF Core **global query filters** (`modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted)`) applied to every entity, so soft-deleted rows are excluded from normal queries automatically.
- "Delete" operations in the Application layer set `IsDeleted = true` (and `ModifiedAtUtc`/`ModifiedByUserId`) rather than issuing a SQL `DELETE`.
- Reports that need historical accuracy (e.g. "sales for Q1") explicitly ignore the filter (`IgnoreQueryFilters()`) where appropriate, since a since-deleted product should still show up in past sales figures.
- Unique constraints (e.g. `Products.Sku`, `Products.Barcode`) are scoped to exclude soft-deleted rows, so a deleted product's SKU can be reused.

---

## Cascade Rules

| Relationship | On parent delete | Reasoning |
|---|---|---|
| SalesOrder → SaleItems | Cascade (soft) | Line items have no meaning without their parent order |
| SalesOrder → Invoices | Restrict | An invoiced order shouldn't be deletable without explicitly voiding first |
| PurchaseOrder → PurchaseItems | Cascade (soft) | Same reasoning as SaleItems |
| Product → SaleItems / PurchaseItems / StockMovements | Restrict | Historical transaction records must never disappear because a product was deleted |
| Category → Products | Set null | Deleting a category shouldn't delete or block deletion of its products; `CategoryId` becomes null |
| Warehouse → StockLevels | Restrict | A warehouse with stock on hand can't be deleted until stock is transferred out or zeroed |
| Customer → SalesOrders | Restrict | Preserve sales history even if a customer record is later deleted |

"Cascade (soft)" means the cascade only ever sets `IsDeleted = true` on children — never a hard cascade delete, per the [Soft-Delete Strategy](#soft-delete-strategy) above.

---

## Migration Notes

- **v1.0** ships with the schema above as a **single-entry bookkeeping model** (`Transactions.Amount` signed positive/negative against one `Account`). This is intentionally simpler than full double-entry accounting to keep the Accounting module shippable for v1.0; a double-entry ledger (`DebitAccountId` / `CreditAccountId` per transaction) is a candidate schema change for v2.0 if the accounting module needs to support proper trial balances.
- **Multi-currency** is out of scope for v1.0; `Money`-typed columns assume a single business-wide base currency. Introducing multi-currency later would mean adding a `CurrencyCode` column to monetary tables and a value-object change in the Domain layer — noted here so it isn't a surprise when v3.0 cloud/multi-branch work begins.
- **Multi-branch (v3.0)** — most tables would need a `BranchId` scoping column (or rely on `WarehouseId` doubling as branch scope, if the business model maps 1:1). This is deferred rather than built into v1.0 to avoid over-engineering a single-workstation product.
