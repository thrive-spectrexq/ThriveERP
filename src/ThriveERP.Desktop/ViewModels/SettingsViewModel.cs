using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using ThriveERP.Application.Features.Settings;

namespace ThriveERP.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private string _title = "System Settings";

    [ObservableProperty]
    private string _companyName = "Thrive Inc.";

    [ObservableProperty]
    private string _currencySymbol = "$";

    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel()
    {
        _mediator = App.Services!.GetRequiredService<IMediator>();
        LoadSettingsAsync().ConfigureAwait(false);
    }

    private async Task LoadSettingsAsync()
    {
        var companyName = await _mediator.Send(new GetSettingQuery("BusinessName"));
        if (!string.IsNullOrEmpty(companyName))
        {
            CompanyName = companyName;
        }
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        if (Avalonia.Application.Current is not null)
        {
            Avalonia.Application.Current.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            await _mediator.Send(new UpdateSettingCommand("BusinessName", CompanyName));
            StatusMessage = "Settings saved successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
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
            
            StatusMessage = $"Backup successful: {path}";
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Backup failed: {ex.Message}";
        }
    }
}
