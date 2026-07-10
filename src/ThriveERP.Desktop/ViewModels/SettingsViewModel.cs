using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ThriveERP.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "System Settings";

    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private string _companyName = "Thrive Inc.";

    [ObservableProperty]
    private string _currencySymbol = "$";

    partial void OnIsDarkModeChanged(bool value)
    {
        if (Application.Current is not null)
        {
            Application.Current.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // Mock save action
    }
}
