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

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices();
                services.AddInfrastructureServices(context.Configuration);
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<MainWindow>();
                
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ProductsViewModel>();
                services.AddTransient<AddProductViewModel>();
                services.AddTransient<SalesViewModel>();
                services.AddTransient<AddSalesOrderViewModel>();
                services.AddTransient<CustomersViewModel>();
                services.AddTransient<AddCustomerViewModel>();
                services.AddTransient<SettingsViewModel>();
            })
            .Build();

        Services = _host.Services;

        // Ensure database is created (for dev purposes)
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ThriveERP.Infrastructure.Data.ThriveErpDbContext>();
            db.Database.EnsureCreated();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // BindingPlugins.DataValidators.RemoveAt(0); // Removed for Avalonia compatibility
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}