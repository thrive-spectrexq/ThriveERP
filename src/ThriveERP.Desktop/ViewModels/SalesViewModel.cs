using CommunityToolkit.Mvvm.ComponentModel;

namespace ThriveERP.Desktop.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Sales Management";
}
