using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Features.Customers;
using ThriveERP.Application.Features.HR;
using ThriveERP.Application.Features.Products;
using ThriveERP.Application.Features.Sales;
using ThriveERP.Application.Features.Settings;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Desktop.Services;

namespace ThriveERP.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private ViewModelBase _currentPage = null!;

    [ObservableProperty]
    private ListItemTemplate? _selectedListItem;

    [ObservableProperty]
    private string _currentUserName = "Admin User";

    public ObservableCollection<ListItemTemplate> Items { get; } = new();

    public SalesViewModel SalesViewModel { get; }
    public EmployeeViewModel EmployeeViewModel { get; }

    // --- GLOBAL SEARCH SYSTEM ---
    [ObservableProperty]
    private string _globalSearchQuery = string.Empty;

    public ObservableCollection<GlobalSearchResultViewModel> GlobalSearchResults { get; } = new();

    [ObservableProperty]
    private bool _isSearchDropdownOpen;

    // --- NOTIFICATIONS & ALERTS SYSTEM ---
    [ObservableProperty]
    private bool _isNotificationFlyoutOpen;

    public ObservableCollection<NotificationItemViewModel> Notifications { get; } = new();

    [ObservableProperty]
    private int _unreadNotificationCount;

    // --- TOAST FEEDBACK ---
    [ObservableProperty]
    private string _toastMessage = string.Empty;

    [ObservableProperty]
    private bool _isToastVisible;

    // --- THEME SELECTOR ---
    public ObservableCollection<string> AvailableThemes { get; } = new(new[]
    {
        "Cool Slate (Light)",
        "Soft Dark",
        "Dimmed Gray (Light)",
        "Warm Neutral (Light)"
    });

    [ObservableProperty]
    private string _selectedTheme = "Cool Slate (Light)";

    public static MainWindowViewModel? Instance { get; private set; }

    private readonly ILicenseService _licenseService;

    [ObservableProperty]
    private bool _isLicenseLocked;

    [ObservableProperty]
    private string _licenseKeyInput = string.Empty;

    [ObservableProperty]
    private string _licenseErrorMessage = string.Empty;

    public MainWindowViewModel(IMediator mediator)
    {
        Instance = this;
        _mediator = mediator;
        _currentPage = new DashboardViewModel(mediator);
        SalesViewModel = App.Services!.GetRequiredService<SalesViewModel>();
        EmployeeViewModel = App.Services!.GetRequiredService<EmployeeViewModel>();

        // Apply Cool Slate Light Theme by default
        ThemeManager.ApplyTheme(AppTheme.CoolSlate);

        _licenseService = App.Services!.GetRequiredService<ILicenseService>();

        SetupForRole(App.CurrentRole);
        InitializeNotifications();
        
        _ = InitializeLicenseAsync();
    }

    private async Task InitializeLicenseAsync()
    {
        try
        {
            var licenseKey = await _mediator.Send(new GetSettingQuery("LicenseKey"));
            CheckLicenseStatus(licenseKey ?? string.Empty);
        }
        catch
        {
            // If DB is not ready or query fails, assume locked for safety
            IsLicenseLocked = true;
            LicenseErrorMessage = "System startup error. Please enter a valid license.";
        }
    }

    private void CheckLicenseStatus(string licenseKey)
    {
        var days = _licenseService.GetDaysRemaining(licenseKey);
        if (days <= 0)
        {
            IsLicenseLocked = true;
            LicenseErrorMessage = "No valid license found or license has expired. Please activate ThriveERP.";
        }
        else if (days <= 30)
        {
            IsLicenseLocked = false;
            ShowToast($"License expiring in {days} days. Please renew soon.");
        }
        else
        {
            IsLicenseLocked = false;
        }
    }

    [RelayCommand]
    private async Task ActivateLicenseAsync()
    {
        LicenseErrorMessage = string.Empty;
        var validation = _licenseService.ValidateLicense(LicenseKeyInput);
        if (validation.IsValid)
        {
            await _mediator.Send(new UpdateSettingCommand("LicenseKey", LicenseKeyInput));
            IsLicenseLocked = false;
            ShowToast($"License Activated! Welcome, {validation.CustomerName}. Expires: {validation.ExpirationDate:d}");
        }
        else
        {
            LicenseErrorMessage = validation.ErrorMessage ?? "Invalid license key.";
        }
    }

    public void SetupForRole(string roleName)
    {
        CurrentUserName = roleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin User" : "Cashier User";

        Items.Clear();
        Items.Add(new ListItemTemplate(typeof(DashboardViewModel), "Dashboard", "Home"));

        if (roleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
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
        else if (roleName.Equals("Cashier", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Teller", StringComparison.OrdinalIgnoreCase))
        {
            Items.Add(new ListItemTemplate(typeof(SalesViewModel), "Sales", "ShoppingCart"));
            Items.Add(new ListItemTemplate(typeof(CustomersViewModel), "Customers", "People"));
            Items.Add(new ListItemTemplate(typeof(SettingsViewModel), "Settings", "Settings"));
        }

        SelectedListItem = Items[0];
    }

    private void InitializeNotifications()
    {
        Notifications.Clear();
        UpdateUnreadCount();
    }

    public void AddNotification(string title, string message, string icon = "🔔", string badgeColor = "#3B82F6", Type? targetType = null)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Notifications.Insert(0, new NotificationItemViewModel(title, message, "Just now", icon, badgeColor, targetType));
            UpdateUnreadCount();
        });
    }

    private void UpdateUnreadCount()
    {
        UnreadNotificationCount = Notifications.Count(n => !n.IsRead);
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (value.Contains("Cool Slate")) ThemeManager.ApplyTheme(AppTheme.CoolSlate);
        else if (value.Contains("Warm Neutral")) ThemeManager.ApplyTheme(AppTheme.WarmNeutral);
        else if (value.Contains("Soft Dark")) ThemeManager.ApplyTheme(AppTheme.SoftDark);
        else ThemeManager.ApplyTheme(AppTheme.DimmedGray);
    }

    partial void OnGlobalSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
        {
            GlobalSearchResults.Clear();
            IsSearchDropdownOpen = false;
            return;
        }

        _ = ExecuteGlobalSearchAsync(value.ToLower().Trim());
    }

    private async Task ExecuteGlobalSearchAsync(string query)
    {
        GlobalSearchResults.Clear();

        try
        {
            // 1. Products
            var products = await _mediator.Send(new GetAllProductsQuery());
            foreach (var p in products.Where(p => p.Name.ToLower().Contains(query) || p.Sku.ToLower().Contains(query)).Take(4))
            {
                GlobalSearchResults.Add(new GlobalSearchResultViewModel(p.Name, $"SKU: {p.Sku} | Price: {p.SellingPrice:C}", "Product", "📦", typeof(ProductsViewModel)));
            }

            // 2. Customers
            var customers = await _mediator.Send(new GetAllCustomersQuery());
            foreach (var c in customers.Where(c => c.Name.ToLower().Contains(query) || (c.Email != null && c.Email.ToLower().Contains(query))).Take(3))
            {
                GlobalSearchResults.Add(new GlobalSearchResultViewModel(c.Name, $"Email: {c.Email} | Phone: {c.Phone}", "Customer", "👤", typeof(CustomersViewModel)));
            }

            // 3. Sales Orders
            var sales = await _mediator.Send(new GetAllSalesOrdersQuery());
            foreach (var s in sales.Where(s => s.OrderNumber.ToLower().Contains(query)).Take(3))
            {
                GlobalSearchResults.Add(new GlobalSearchResultViewModel($"Order #{s.OrderNumber}", $"Total: {s.GrandTotal:C} | Status: {s.Status}", "Sales Order", "🛒", typeof(SalesViewModel)));
            }

            IsSearchDropdownOpen = GlobalSearchResults.Any();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Global search error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SelectSearchResult(GlobalSearchResultViewModel result)
    {
        if (result == null) return;
        IsSearchDropdownOpen = false;
        GlobalSearchQuery = string.Empty;
        NavigateToType(result.ViewModelType);
        ShowToast($"Navigated to {result.Category}: {result.Title}");
    }

    public void NavigateToType(Type targetType)
    {
        var matchedItem = Items.FirstOrDefault(i => i.ModelType == targetType);
        if (matchedItem != null)
        {
            SelectedListItem = matchedItem;
        }
    }

    [RelayCommand]
    private void ToggleNotifications()
    {
        IsNotificationFlyoutOpen = !IsNotificationFlyoutOpen;
    }

    [RelayCommand]
    private void MarkAllNotificationsRead()
    {
        foreach (var n in Notifications) n.IsRead = true;
        UpdateUnreadCount();
        ShowToast("All notifications marked as read");
    }

    [RelayCommand]
    private void ClearNotifications()
    {
        Notifications.Clear();
        UpdateUnreadCount();
        IsNotificationFlyoutOpen = false;
        ShowToast("Notifications cleared");
    }

    [RelayCommand]
    private void SelectNotification(NotificationItemViewModel notification)
    {
        if (notification == null) return;
        notification.IsRead = true;
        UpdateUnreadCount();
        IsNotificationFlyoutOpen = false;

        if (notification.NavigationTargetType != null)
        {
            NavigateToType(notification.NavigationTargetType);
        }
    }

    public void ShowToast(string message)
    {
        ToastMessage = message;
        IsToastVisible = true;
        
        Task.Delay(3000).ContinueWith(_ =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsToastVisible = false;
            });
        });
    }

    partial void OnSelectedListItemChanged(ListItemTemplate? value)
    {
        if (value is null || value.ModelType is null) return;

        if (App.Services?.GetService(value.ModelType) is ViewModelBase vm)
        {
            CurrentPage = vm;
        }
        else
        {
            if (value.ModelType == typeof(DashboardViewModel))
            {
                CurrentPage = new DashboardViewModel(_mediator);
            }
            else
            {
                CurrentPage = (ViewModelBase)Activator.CreateInstance(value.ModelType)!;
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
    public ListItemTemplate(Type? type, string label, string icon)
    {
        ModelType = type;
        Label = label;
        Icon = icon;
    }

    public string Label { get; }
    public Type? ModelType { get; }
    public string Icon { get; }
}
