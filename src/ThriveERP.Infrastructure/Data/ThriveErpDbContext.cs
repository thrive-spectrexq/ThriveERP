namespace ThriveERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ThriveERP.Domain.Entities;

public class ThriveErpDbContext : DbContext
{
    public ThriveErpDbContext(DbContextOptions<ThriveErpDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductSupplier> ProductSuppliers => Set<ProductSupplier>();
    
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Return> Returns => Set<Return>();
    
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();
    
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Batch> Batches => Set<Batch>();
    
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Expense> Expenses => Set<Expense>();
    
    public DbSet<Employee> Employees => Set<Employee>();
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ThriveErpDbContext).Assembly);
        modelBuilder.Entity<ProductSupplier>().HasKey(ps => new { ps.ProductId, ps.SupplierId });
        modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
        modelBuilder.Entity<Role>().HasData(
            new Role 
            { 
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), 
                Name = "Administrator", 
                Description = "System Administrator" 
            }
        );

        modelBuilder.Entity<Warehouse>().HasData(
            new Warehouse 
            { 
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), 
                Name = "Main Warehouse", 
                Location = "HQ", 
                IsDefault = true 
            }
        );
    }
}
