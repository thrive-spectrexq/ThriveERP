namespace ThriveERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ThriveERP.Domain.Entities;

public class ThriveErpDbContext : DbContext
{
    public ThriveErpDbContext(DbContextOptions<ThriveErpDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ThriveErpDbContext).Assembly);
    }
}
