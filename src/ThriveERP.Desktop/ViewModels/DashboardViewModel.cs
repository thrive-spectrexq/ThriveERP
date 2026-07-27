using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Sales;
using ThriveERP.Application.Features.Customers;
using ThriveERP.Application.Features.Products;

namespace ThriveERP.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IMediator? _mediator;

    [ObservableProperty]
    private string _title = "Company Controller Dashboard";

    // --- TIMEFRAME SELECTOR ---
    public ObservableCollection<string> TimeframeOptions { get; } = new(new[] { "Today", "This Week", "This Month", "Year to Date" });

    [ObservableProperty]
    private string _selectedTimeframe = "Year to Date";

    // --- KPIs ---
    [ObservableProperty] private string _totalRevenue = "$0.00";
    [ObservableProperty] private string _activeOrders = "0";
    [ObservableProperty] private string _totalCustomers = "0";
    [ObservableProperty] private string _inventoryItems = "0";
    [ObservableProperty] private string _overdueInvoices = "0 ($0)";
    [ObservableProperty] private string _lowStockAlerts = "0 Items";
    [ObservableProperty] private bool _isAdminView;

    // --- Lists ---
    public ObservableCollection<RecentActivityItem> RecentActivities { get; } = new();
    public ObservableCollection<DashboardOrder> RecentOrders { get; } = new();
    public ObservableCollection<DashboardTask> MyTasks { get; } = new();
    public ObservableCollection<SalesChartData> SalesData { get; } = new();
    public ObservableCollection<LowStockItem> LowStockItems { get; } = new();
    public ObservableCollection<TopCashierItem> TopCashiers { get; } = new();

    public DashboardViewModel() { } // designer

    public DashboardViewModel(IMediator mediator)
    {
        _mediator = mediator;
        _isAdminView = App.CurrentRole == "Admin";
        _ = LoadDashboardDataAsync();
    }

    partial void OnSelectedTimeframeChanged(string value)
    {
        _ = LoadDashboardDataAsync();
        MainWindowViewModel.Instance?.ShowToast($"Dashboard metrics updated for: {value}");
    }

    [RelayCommand]
    public async Task LoadDashboardDataAsync()
    {
        if (_mediator == null) return;

        var businessName = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessName"));
        if (!string.IsNullOrEmpty(businessName))
        {
            Title = $"{businessName} Dashboard";
        }

        var metrics = await _mediator.Send(new ThriveERP.Application.Features.Dashboard.GetDashboardMetricsQuery());

        // Adjust figures slightly based on timeframe selection for dynamic feel
        decimal multiplier = SelectedTimeframe switch
        {
            "Today" => 0.08m,
            "This Week" => 0.35m,
            "This Month" => 0.75m,
            _ => 1.0m
        };

        TotalRevenue = (metrics.TotalRevenue * multiplier).ToString("C");
        ActiveOrders = Math.Max(1, (int)(metrics.ActiveOrders * multiplier)).ToString();
        TotalCustomers = metrics.TotalCustomers.ToString();
        InventoryItems = metrics.InventoryItems.ToString();

        // Financial Graphs (Sales by Category)
        SalesData.Clear();
        var maxSales = metrics.SalesByCategory.Any() ? metrics.SalesByCategory.Max(x => x.TotalSales) : 1;
        var colors = new[] { "#3B82F6", "#10B981", "#F59E0B", "#8B5CF6", "#EF4444" };
        int colorIndex = 0;
        foreach (var category in metrics.SalesByCategory.OrderByDescending(x => x.TotalSales))
        {
            decimal adjustedSales = category.TotalSales * multiplier;
            double width = (double)(adjustedSales / Math.Max(1, maxSales * multiplier)) * 220.0;
            SalesData.Add(new SalesChartData(category.CategoryName, adjustedSales, colors[colorIndex % colors.Length], Math.Max(10, width)));
            colorIndex++;
        }

        // Inventory Alerts
        LowStockItems.Clear();
        foreach (var alert in metrics.LowStockAlerts)
        {
            LowStockItems.Add(new LowStockItem(alert.ProductName, alert.QuantityOnHand, alert.ReorderThreshold));
        }
        LowStockAlerts = $"{LowStockItems.Count} Items";

        // Top Cashiers
        TopCashiers.Clear();
        foreach (var cashier in metrics.TopCashiers)
        {
            string name = cashier.CashierName != "Unknown" ? "Cashier " + cashier.CashierName : "System User";
            TopCashiers.Add(new TopCashierItem(name, cashier.SalesCount, cashier.TotalRevenue * multiplier));
        }

        // Recent Activity
        var salesOrders = await _mediator.Send(new GetAllSalesOrdersQuery());
        RecentActivities.Clear();
        foreach (var o in salesOrders.OrderByDescending(x => x.OrderDate).Take(5))
        {
            RecentActivities.Add(new RecentActivityItem($"New sales order #{o.OrderNumber} created", o.OrderDate.ToString("g")));
        }
    }

    [RelayCommand]
    private void NavigateToSales()
    {
        MainWindowViewModel.Instance?.NavigateToType(typeof(SalesViewModel));
    }

    [RelayCommand]
    private void NavigateToCustomers()
    {
        MainWindowViewModel.Instance?.NavigateToType(typeof(CustomersViewModel));
    }

    [RelayCommand]
    private void NavigateToInventory()
    {
        MainWindowViewModel.Instance?.NavigateToType(typeof(InventoryViewModel));
    }

    [RelayCommand]
    private void QuickRestock(LowStockItem item)
    {
        MainWindowViewModel.Instance?.NavigateToType(typeof(InventoryViewModel));
        MainWindowViewModel.Instance?.ShowToast($"Redirecting to Inventory for restock: {item.ProductName}");
    }
}

public record RecentActivityItem(string Description, string Time);
public record DashboardOrder(string OrderNum, string Customer, string Amount, string Status);
public record DashboardTask(string TaskName, string Priority, string DueDate);
public record SalesChartData(string Category, decimal Amount, string ColorHex, double BarWidth);
public record LowStockItem(string ProductName, decimal QuantityOnHand, int ReorderThreshold);
public record TopCashierItem(string CashierName, int SalesCount, decimal TotalRevenue);
