using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ThriveERP.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "System Settings";

    [ObservableProperty]
    private ObservableCollection<string> _settingCategories = new()
    {
        "General Preferences",
        "Company Profile",
        "Taxes & Financials",
        "User Management",
        "Integrations"
    };

    [ObservableProperty]
    private string _selectedCategory = "General Preferences";

    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private string _companyName = "Thrive Inc.";

    [ObservableProperty]
    private string _currencySymbol = "$";

    [ObservableProperty]
    private string _taxRate = "15.0";

    partial void OnIsDarkModeChanged(bool value)
    {
        if (Avalonia.Application.Current is not null)
        {
            Avalonia.Application.Current.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // Mock save action
    }

    [RelayCommand]
    private async Task BackupDatabaseAsync()
    {
        try
        {
            var backupService = App.Services!.GetRequiredService<ThriveERP.Application.Common.Interfaces.IBackupService>();
            var desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            var path = System.IO.Path.Combine(desktop, $"ThriveERP_Backup_{System.DateTime.Now:yyyyMMdd_HHmmss}.db");
            
            await backupService.BackupDatabaseAsync(path, "SecureP@ssw0rd123!");
            
            // Success
            Console.WriteLine($"Backup successful: {path}");
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Backup failed: {ex.Message}");
        }
    }
}
