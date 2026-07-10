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

    // --- KPIs ---
    [ObservableProperty] private string _totalRevenue = "$0.00";
    [ObservableProperty] private string _activeOrders = "0";
    [ObservableProperty] private string _totalCustomers = "0";
    [ObservableProperty] private string _inventoryItems = "0";
    [ObservableProperty] private string _overdueInvoices = "0 ($0)";
    [ObservableProperty] private string _lowStockAlerts = "0 Items";

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
        _ = LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        if (_mediator == null) return;

        var salesOrders = await _mediator.Send(new GetAllSalesOrdersQuery());
        var customers = await _mediator.Send(new GetAllCustomersQuery());
        var products = await _mediator.Send(new GetAllProductsQuery());
        // For actual inventory alerts, we would need stock levels, but let's approximate
        // since we might not have the full stock levels loaded here.
        var stockLevels = await _mediator.Send(new ThriveERP.Application.Features.Inventory.GetStockLevelsQuery(null, null));

        var revenue = salesOrders.Sum(o => o.GrandTotal);
        TotalRevenue = revenue.ToString("C");

        var active = salesOrders.Count(o => o.Status != "Voided");
        ActiveOrders = active.ToString();

        TotalCustomers = customers.Count.ToString();
        InventoryItems = products.Count.ToString();

        RecentActivities.Clear();
        foreach (var o in salesOrders.OrderByDescending(x => x.OrderDate).Take(5))
        {
            RecentActivities.Add(new RecentActivityItem($"New sales order #{o.OrderNumber} created", o.OrderDate.ToString("g")));
        }

        // Financial Graphs (mocked data for now, scaled to $ revenue)
        SalesData.Clear();
        SalesData.Add(new SalesChartData("Electronics", 45000, "#3B82F6", 150));
        SalesData.Add(new SalesChartData("Furniture", 32000, "#10B981", 100));
        SalesData.Add(new SalesChartData("Apparel", 24000, "#F59E0B", 80));

        // Inventory Alerts (hardcoded threshold of 10 for demo)
        LowStockItems.Clear();
        foreach (var stock in stockLevels.Where(s => s.QuantityOnHand <= 10).Take(5))
        {
            LowStockItems.Add(new LowStockItem(stock.ProductName, stock.QuantityOnHand, 10));
        }
        LowStockAlerts = $"{LowStockItems.Count} Items";

        // Top Cashiers
        TopCashiers.Clear();
        TopCashiers.Add(new TopCashierItem("Admin User", salesOrders.Count, revenue));
        // Real implementation would group by CreatedByUserId and match with employee names
    }
}

public record RecentActivityItem(string Description, string Time);
public record DashboardOrder(string OrderNum, string Customer, string Amount, string Status);
public record DashboardTask(string TaskName, string Priority, string DueDate);
public record SalesChartData(string Category, decimal Amount, string ColorHex, double BarWidth);
public record LowStockItem(string ProductName, decimal QuantityOnHand, int ReorderThreshold);
public record TopCashierItem(string CashierName, int SalesCount, decimal TotalRevenue);
