using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Application.Features.Settings;
using ThriveERP.Desktop.Services;

namespace ThriveERP.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IPosPrinterService _printerService;

    [ObservableProperty]
    private string _title = "System Settings & Customization";

    [ObservableProperty]
    private string _companyName = "Thrive Inc.";

    [ObservableProperty]
    private string _companyPhone = "+1 (555) 234-5678";

    [ObservableProperty]
    private string _companyAddress = "123 Business Way, Suite 100, Cityville";

    [ObservableProperty]
    private string _companyTaxId = "TAX-99201-X";

    [ObservableProperty]
    private string _currencySymbol = "$";

    [ObservableProperty]
    private decimal _defaultTaxRate = 10.0m;

    [ObservableProperty]
    private string _receiptFooterNote = "Thank you for shopping with ThriveERP!";

    // --- POS PRINTER HARDWARE SETTINGS ---
    public ObservableCollection<string> InstalledPrinters { get; } = new();

    [ObservableProperty]
    private string _selectedPrinter = "Default System Printer";

    public ObservableCollection<string> PaperWidthOptions { get; } = new(new[] { "80mm (Standard)", "58mm (Compact)" });

    [ObservableProperty]
    private string _selectedPaperWidth = "80mm (Standard)";

    [ObservableProperty]
    private bool _autoCutPaper = true;

    [ObservableProperty]
    private bool _openCashDrawer = true;

    public ObservableCollection<string> AvailableThemes { get; } = new(new[]
    {
        "Cool Slate (Light)",
        "Soft Dark",
        "Dimmed Gray (Light)",
        "Warm Neutral (Light)"
    });

    [ObservableProperty]
    private string _selectedTheme = "Cool Slate (Light)";

    [ObservableProperty]
    private bool _enableOfflineSync = true;

    [ObservableProperty]
    private bool _enableSoundEffects = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel()
    {
        _mediator = App.Services!.GetRequiredService<IMediator>();
        _printerService = App.Services!.GetRequiredService<IPosPrinterService>();

        if (MainWindowViewModel.Instance != null)
        {
            SelectedTheme = MainWindowViewModel.Instance.SelectedTheme;
        }

        LoadPrinters();
        _ = LoadSettingsAsync();
    }

    private void LoadPrinters()
    {
        InstalledPrinters.Clear();
        var printers = _printerService.GetInstalledPrinters();
        foreach (var p in printers) InstalledPrinters.Add(p);
        
        if (InstalledPrinters.Count > 0)
        {
            SelectedPrinter = InstalledPrinters[0];
        }
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (MainWindowViewModel.Instance != null && MainWindowViewModel.Instance.SelectedTheme != value)
        {
            MainWindowViewModel.Instance.SelectedTheme = value;
        }
    }

    private async Task LoadSettingsAsync()
    {
        var companyName = await _mediator.Send(new GetSettingQuery("BusinessName"));
        if (!string.IsNullOrEmpty(companyName)) CompanyName = companyName;

        var phone = await _mediator.Send(new GetSettingQuery("BusinessPhone"));
        if (!string.IsNullOrEmpty(phone)) CompanyPhone = phone;

        var address = await _mediator.Send(new GetSettingQuery("BusinessAddress"));
        if (!string.IsNullOrEmpty(address)) CompanyAddress = address;

        var taxId = await _mediator.Send(new GetSettingQuery("BusinessTaxId"));
        if (!string.IsNullOrEmpty(taxId)) CompanyTaxId = taxId;

        var footer = await _mediator.Send(new GetSettingQuery("ReceiptFooterNote"));
        if (!string.IsNullOrEmpty(footer)) ReceiptFooterNote = footer;
    }

    [RelayCommand]
    private async Task PrintTestReceiptAsync()
    {
        try
        {
            StatusMessage = $"Printing test receipt to '{SelectedPrinter}'...";
            string pWidth = SelectedPaperWidth.Contains("58mm") ? "58mm" : "80mm";
            
            bool result = await _printerService.PrintTestReceiptAsync(SelectedPrinter, pWidth, CompanyName);

            if (result)
            {
                StatusMessage = $"Test receipt sent to '{SelectedPrinter}'";
                MainWindowViewModel.Instance?.ShowToast($"Test receipt printed to '{SelectedPrinter}'");
            }
            else
            {
                StatusMessage = $"Printing to '{SelectedPrinter}' failed or exported to PDF";
                MainWindowViewModel.Instance?.ShowToast($"Exported test receipt to PDF/Downloads");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Printer error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            await _mediator.Send(new UpdateSettingCommand("BusinessName", CompanyName));
            await _mediator.Send(new UpdateSettingCommand("BusinessPhone", CompanyPhone));
            await _mediator.Send(new UpdateSettingCommand("BusinessAddress", CompanyAddress));
            await _mediator.Send(new UpdateSettingCommand("BusinessTaxId", CompanyTaxId));
            await _mediator.Send(new UpdateSettingCommand("ReceiptFooterNote", ReceiptFooterNote));

            StatusMessage = "Settings saved successfully.";
            MainWindowViewModel.Instance?.ShowToast("Business info & receipt header settings saved");
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
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var path = System.IO.Path.Combine(desktop, $"ThriveERP_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
            
            await backupService.BackupDatabaseAsync(path, "SecureP@ssw0rd123!");
            
            StatusMessage = $"Backup created: {path}";
            MainWindowViewModel.Instance?.ShowToast($"Database backup saved to Desktop");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Backup failed: {ex.Message}";
        }
    }
}
