using CommunityToolkit.Mvvm.ComponentModel;

namespace ThriveERP.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Dashboard";

    [ObservableProperty]
    private string _welcomeMessage = "Welcome to ThriveERP Dashboard";
}
