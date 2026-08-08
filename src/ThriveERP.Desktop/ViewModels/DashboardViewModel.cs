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
using Avalonia.Threading;

namespace ThriveERP.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IMediator? _mediator;
    private DispatcherTimer? _posSimTimer;

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

    // --- Chart Data ---
    public ObservableCollection<NativeChartItem> RevenueChartData { get; } = new();
    public ObservableCollection<NativeChartItem> CategoryChartData { get; } = new();

    // --- Lists ---
    public ObservableCollection<LivePosActivityItem> LivePosFeed { get; } = new();
    public ObservableCollection<LowStockItem> LowStockItems { get; } = new();
    public ObservableCollection<TopCashierItem> TopCashiers { get; } = new();

    public DashboardViewModel() { } // designer

    public DashboardViewModel(IMediator mediator)
    {
        _mediator = mediator;
        _isAdminView = App.CurrentRole == "Admin";
        
        _ = LoadDashboardDataAsync();

        if (_isAdminView)
        {
            StartLivePosFeedSimulator();
        }
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

        // 1. UPDATE REVENUE BAR CHART (Native)
        RevenueChartData.Clear();
        var rnd = new Random();
        var baseVal = (double)(metrics.TotalRevenue * multiplier) / 7.0;
        string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        var rawValues = new double[7];
        double maxRev = 1;
        
        for(int i = 0; i < 7; i++) 
        {
            rawValues[i] = Math.Max(10, baseVal + rnd.NextDouble() * 500 - 250);
            if (rawValues[i] > maxRev) maxRev = rawValues[i];
        }

        for (int i = 0; i < 7; i++)
        {
            double height = (rawValues[i] / maxRev) * 180.0;
            RevenueChartData.Add(new NativeChartItem(days[i], rawValues[i], height, "#10B981"));
        }

        // 2. UPDATE CATEGORY BAR CHART (Native)
        CategoryChartData.Clear();
        var colors = new[] { "#3B82F6", "#10B981", "#F59E0B", "#8B5CF6", "#EF4444" };
        var topCats = metrics.SalesByCategory.OrderByDescending(x => x.TotalSales).Take(5).ToList();
        double maxCat = topCats.Any() ? (double)topCats.Max(x => x.TotalSales) * (double)multiplier : 1;
        
        int colorIdx = 0;
        foreach (var category in topCats)
        {
            double amount = (double)(category.TotalSales * multiplier);
            double width = (amount / maxCat) * 200.0;
            CategoryChartData.Add(new NativeChartItem(category.CategoryName, amount, Math.Max(10, width), colors[colorIdx % colors.Length]));
            colorIdx++;
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
    }

    private void StartLivePosFeedSimulator()
    {
        LivePosFeed.Clear();
        LivePosFeed.Add(new LivePosActivityItem("ORD-7001", "Cashier Sarah", "$124.50", "Completed", "Just now"));
        LivePosFeed.Add(new LivePosActivityItem("ORD-7002", "Cashier Mike", "$45.00", "Processing", "1m ago"));

        _posSimTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        _posSimTimer.Tick += (s, e) =>
        {
            var rnd = new Random();
            var orderNum = "ORD-" + rnd.Next(7003, 9999);
            var amount = "$" + (rnd.NextDouble() * 200 + 15).ToString("0.00");
            var cashiers = new[] { "Cashier Sarah", "Cashier Mike", "Admin", "Cashier Jane" };
            
            LivePosFeed.Insert(0, new LivePosActivityItem(orderNum, cashiers[rnd.Next(cashiers.Length)], amount, "Completed", "Just now"));
            
            if (LivePosFeed.Count > 10)
            {
                LivePosFeed.RemoveAt(LivePosFeed.Count - 1);
            }
        };
        _posSimTimer.Start();
    }

    [RelayCommand]
    private void NavigateToSales() => MainWindowViewModel.Instance?.NavigateToType(typeof(SalesViewModel));

    [RelayCommand]
    private void NavigateToCustomers() => MainWindowViewModel.Instance?.NavigateToType(typeof(CustomersViewModel));

    [RelayCommand]
    private void NavigateToInventory() => MainWindowViewModel.Instance?.NavigateToType(typeof(InventoryViewModel));

    [RelayCommand]
    private void QuickRestock(LowStockItem item)
    {
        MainWindowViewModel.Instance?.NavigateToType(typeof(InventoryViewModel));
        MainWindowViewModel.Instance?.ShowToast($"Redirecting to Inventory for restock: {item.ProductName}");
    }
}

public record NativeChartItem(string Label, double Value, double Size, string ColorHex);
public record LivePosActivityItem(string OrderId, string Cashier, string Amount, string Status, string Time);
public record LowStockItem(string ProductName, decimal QuantityOnHand, int ReorderThreshold);
public record TopCashierItem(string CashierName, int SalesCount, decimal TotalRevenue);
