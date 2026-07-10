using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ThriveERP.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    public ObservableCollection<ListItemTemplate> Items { get; } = new()
    {
        new ListItemTemplate(typeof(DashboardViewModel), "Dashboard", "Home"),
        new ListItemTemplate(typeof(ProductsViewModel), "Products", "Box"),
        new ListItemTemplate(typeof(SalesViewModel), "Sales", "ShoppingCart"),
        new ListItemTemplate(typeof(CustomersViewModel), "Customers", "People"),
        new ListItemTemplate(typeof(SettingsViewModel), "Settings", "Settings")
    };

    [ObservableProperty]
    private ListItemTemplate? _selectedListItem;

    public MainWindowViewModel(IMediator mediator)
    {
        _mediator = mediator;
        _currentPage = new DashboardViewModel();
        SelectedListItem = Items[0];
    }

    partial void OnSelectedListItemChanged(ListItemTemplate? value)
    {
        if (value is null || value.ModelType is null) return;

        // Use the DI container to get the view model if it's registered
        if (App.Services?.GetService(value.ModelType) is ViewModelBase vm)
        {
            CurrentPage = vm;
        }
        else
        {
            // Fallback to Activator if not registered or no DI
            CurrentPage = (ViewModelBase)System.Activator.CreateInstance(value.ModelType)!;
        }
    }
}

public class ListItemTemplate
{
    public ListItemTemplate(System.Type? type, string label, string icon)
    {
        ModelType = type;
        Label = label;
        Icon = icon;
    }

    public string Label { get; }
    public System.Type? ModelType { get; }
    public string Icon { get; }
}
