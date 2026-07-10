using CommunityToolkit.Mvvm.ComponentModel;

namespace ThriveERP.Desktop.ViewModels;

public partial class ProductsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Products Management";
}
