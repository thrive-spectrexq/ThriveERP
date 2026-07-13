namespace ThriveERP.Application.Features.Dashboard;
using System.Collections.Generic;

public record DashboardMetricsDto(
    decimal TotalRevenue,
    int ActiveOrders,
    int TotalCustomers,
    int InventoryItems,
    List<SalesByCategoryDto> SalesByCategory,
    List<TopCashierDto> TopCashiers,
    List<LowStockAlertDto> LowStockAlerts
);

public record SalesByCategoryDto(string CategoryName, decimal TotalSales);
public record TopCashierDto(string CashierName, int SalesCount, decimal TotalRevenue);
public record LowStockAlertDto(string ProductName, decimal QuantityOnHand, int ReorderThreshold);
