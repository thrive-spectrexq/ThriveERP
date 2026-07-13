using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ThriveERP.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    public SalesViewModel SalesViewModel { get; }
    public EmployeeViewModel EmployeeViewModel { get; }

    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private string _currentUserName = "Admin User";

    [ObservableProperty]
    private ViewModelBase _currentView = null!;

    public ObservableCollection<ListItemTemplate> Items { get; } = new();

    [ObservableProperty]
    private ListItemTemplate? _selectedListItem;

    public MainWindowViewModel(IMediator mediator)
    {
        _mediator = mediator;
        _currentPage = new DashboardViewModel(mediator);
        SalesViewModel = App.Services!.GetRequiredService<SalesViewModel>();
        EmployeeViewModel = App.Services!.GetRequiredService<EmployeeViewModel>();
        
        // Default to Admin just in case
        SetupForRole("Admin");
    }

    public void SetupForRole(string roleName)
    {
        CurrentUserName = roleName.Equals("Admin", System.StringComparison.OrdinalIgnoreCase) ? "Admin User" : "Cashier User";
        
        Items.Clear();
        Items.Add(new ListItemTemplate(typeof(DashboardViewModel), "Dashboard", "Home"));

        if (roleName.Equals("Admin", System.StringComparison.OrdinalIgnoreCase))
        {
            Items.Add(new ListItemTemplate(typeof(ProductsViewModel), "Products", "Box"));
            Items.Add(new ListItemTemplate(typeof(SalesViewModel), "Sales", "ShoppingCart"));
            Items.Add(new ListItemTemplate(typeof(PurchasingViewModel), "Purchasing", "CartOutline"));
            Items.Add(new ListItemTemplate(typeof(CustomersViewModel), "Customers", "People"));
            Items.Add(new ListItemTemplate(typeof(SuppliersViewModel), "Suppliers", "BuildingFactory"));
            Items.Add(new ListItemTemplate(typeof(EmployeeViewModel), "HR/Employees", "AccountMultiple"));
            Items.Add(new ListItemTemplate(typeof(InventoryViewModel), "Inventory", "BoxMultiple"));
            Items.Add(new ListItemTemplate(typeof(AccountingViewModel), "Accounting", "Calculator"));
            Items.Add(new ListItemTemplate(typeof(ReportsViewModel), "Reports", "FileChart"));
            Items.Add(new ListItemTemplate(typeof(SettingsViewModel), "Settings", "Settings"));
        }
        else if (roleName.Equals("Cashier", System.StringComparison.OrdinalIgnoreCase) || roleName.Equals("Teller", System.StringComparison.OrdinalIgnoreCase))
        {
            Items.Add(new ListItemTemplate(typeof(SalesViewModel), "Sales", "ShoppingCart"));
            Items.Add(new ListItemTemplate(typeof(CustomersViewModel), "Customers", "People"));
            Items.Add(new ListItemTemplate(typeof(SettingsViewModel), "Settings", "Settings"));
        }

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
            // For DashboardViewModel, we must pass the mediator
            if (value.ModelType == typeof(DashboardViewModel))
            {
                CurrentPage = new DashboardViewModel(_mediator);
            }
            else
            {
                // Fallback to Activator if not registered or no DI
                CurrentPage = (ViewModelBase)System.Activator.CreateInstance(value.ModelType)!;
            }
        }
    }

    [RelayCommand]
    private void Logout()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            var loginWindow = new Views.LoginWindow
            {
                DataContext = new ViewModels.LoginViewModel()
            };
            loginWindow.Show();
            desktop.MainWindow?.Close();
            desktop.MainWindow = loginWindow;
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
