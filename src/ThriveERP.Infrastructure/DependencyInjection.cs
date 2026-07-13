namespace ThriveERP.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Infrastructure.Data;
using ThriveERP.Infrastructure.Repositories;
using ThriveERP.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        var dbProvider = config["DatabaseProvider"] ?? "Sqlite";

        services.AddDbContext<ThriveErpDbContext>(options =>
        {
            switch (dbProvider.ToLowerInvariant())
            {
                case "sqlserver":
                    options.UseSqlServer(config.GetConnectionString("SqlServerConnection"));
                    break;
                case "postgres":
                    options.UseNpgsql(config.GetConnectionString("PostgresConnection"));
                    break;
                case "sqlite":
                default:
                    var dbPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "thrive_erp.db");
                    options.UseSqlite($"Data Source={dbPath}");
                    break;
            }
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IStockLevelRepository, StockLevelRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        
        services.AddTransient<IInvoiceGeneratorService, InvoiceGeneratorService>();
        services.AddTransient<IBackupService, BackupService>();
        services.AddTransient<IBarcodeLabelService, BarcodeLabelService>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
