using System;
using Avalonia.Controls;

namespace ThriveERP.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        if (DataContext is ViewModels.LoginViewModel vm)
        {
            vm.OnLoginSuccess = (roleName) =>
            {
                var mainWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MainWindow>(App.Services!);
                if (mainWindow.DataContext is ViewModels.MainWindowViewModel mainVm)
                {
                    mainVm.SetupForRole(roleName);
                }
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = mainWindow;
                }
                mainWindow.Show();
                this.Close();
            };
        }
    }
}
