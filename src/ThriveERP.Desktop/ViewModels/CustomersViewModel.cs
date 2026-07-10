using CommunityToolkit.Mvvm.ComponentModel;

namespace ThriveERP.Desktop.ViewModels;

public partial class CustomersViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Customers Management";
}
