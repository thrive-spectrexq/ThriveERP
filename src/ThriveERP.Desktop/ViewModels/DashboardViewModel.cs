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

    public DashboardViewModel() { } // designer

    public DashboardViewModel(IMediator mediator)
    {
        _mediator = mediator;
        _ = LoadDashboardDataAsync();
        
        // Add some dummy tasks/charts so it doesn't look totally empty
        MyTasks.Add(new DashboardTask("Approve PO-5002", "High", "Today"));
        MyTasks.Add(new DashboardTask("Review Q3 Financials", "Medium", "Tomorrow"));
        
        SalesData.Add(new SalesChartData("Electronics", 45000, "#3B82F6", 150));
        SalesData.Add(new SalesChartData("Furniture", 32000, "#10B981", 100));
    }

    private async Task LoadDashboardDataAsync()
    {
        if (_mediator == null) return;

        var salesOrders = await _mediator.Send(new GetAllSalesOrdersQuery());
        var customers = await _mediator.Send(new GetAllCustomersQuery());
        var products = await _mediator.Send(new GetAllProductsQuery());

        var revenue = salesOrders.Sum(o => o.GrandTotal);
        TotalRevenue = revenue.ToString("C");

        var active = salesOrders.Count(o => o.Status != "Voided");
        ActiveOrders = active.ToString();

        TotalCustomers = customers.Count.ToString();
        InventoryItems = products.Count.ToString();

        RecentActivities.Clear();
        foreach (var o in salesOrders.OrderByDescending(x => x.OrderDate).Take(5))
        {
            RecentActivities.Add(new RecentActivityItem($"New sales order #{o.OrderNumber}", o.OrderDate.ToString("g")));
        }
    }
}

public record RecentActivityItem(string Description, string Time);
public record DashboardOrder(string OrderNum, string Customer, string Amount, string Status);
public record DashboardTask(string TaskName, string Priority, string DueDate);
public record SalesChartData(string Category, decimal Amount, string ColorHex, double BarWidth);
