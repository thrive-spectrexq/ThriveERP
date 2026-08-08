using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using ThriveERP.Application;
using ThriveERP.Desktop.ViewModels;
using ThriveERP.Infrastructure;

namespace ThriveERP.Desktop;

public partial class App : Avalonia.Application
{
    private IHost? _host;
    public static IServiceProvider? Services { get; private set; }
    public static string CurrentRole { get; set; } = "Admin";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices();
                services.AddInfrastructureServices(context.Configuration);
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<MainWindow>();
                services.AddTransient<LoginViewModel>();
                
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ProductsViewModel>();
                services.AddTransient<AddProductViewModel>();
                services.AddTransient<SalesViewModel>();
                services.AddTransient<AddSalesOrderViewModel>();
                services.AddTransient<CustomersViewModel>();
                services.AddTransient<AddCustomerViewModel>();
                services.AddTransient<EmployeeViewModel>();
                services.AddTransient<AddEmployeeViewModel>();
                services.AddTransient<InventoryViewModel>();
                services.AddTransient<SuppliersViewModel>();
                services.AddTransient<AddSupplierViewModel>();
                services.AddTransient<PurchasingViewModel>();
                services.AddTransient<AddPurchaseOrderViewModel>();
                services.AddTransient<AccountingViewModel>();
                services.AddTransient<AddExpenseViewModel>();
                services.AddTransient<ReportsViewModel>();
                services.AddTransient<SettingsViewModel>();
            })
            .Build();

        Services = _host.Services;

        // Run database migrations on startup to keep schema up-to-date
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ThriveERP.Infrastructure.Data.ThriveErpDbContext>();
            Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.Migrate(db.Database);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Views.LoginWindow
            {
                DataContext = Services.GetRequiredService<ViewModels.LoginViewModel>(),
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}