using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThriveERP.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Dashboard overview";

    [ObservableProperty]
    private string _totalRevenue = "$124,500.00";

    [ObservableProperty]
    private string _activeOrders = "45";

    [ObservableProperty]
    private string _totalCustomers = "1,240";

    [ObservableProperty]
    private string _inventoryItems = "3,450";

    public ObservableCollection<RecentActivityItem> RecentActivities { get; } = new()
    {
        new RecentActivityItem("New order #SO-1029 placed", "2 mins ago"),
        new RecentActivityItem("Inventory updated for SKU-402", "15 mins ago"),
        new RecentActivityItem("Customer John Doe added", "1 hour ago"),
        new RecentActivityItem("Payment $500 received", "3 hours ago")
    };
}

public record RecentActivityItem(string Description, string Time);
