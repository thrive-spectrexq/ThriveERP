namespace ThriveERP.Application.Features.Dashboard;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;

public record GetDashboardMetricsQuery() : IRequest<DashboardMetricsDto>;

public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, DashboardMetricsDto>
{
    private readonly ISalesOrderRepository _salesRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockLevelRepository _stockRepository;

    public GetDashboardMetricsQueryHandler(
        ISalesOrderRepository salesRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IStockLevelRepository stockRepository)
    {
        _salesRepository = salesRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _stockRepository = stockRepository;
    }

    public async Task<DashboardMetricsDto> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        var allSales = await _salesRepository.GetAllAsync(cancellationToken);
        var activeSales = allSales.Where(x => x.Status != ThriveERP.Domain.Enums.OrderStatus.Voided).ToList();
        
        decimal totalRevenue = activeSales.Sum(x => x.GrandTotal);
        int activeOrders = activeSales.Count;
        
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        int totalCustomers = customers.Count;
        
        var products = await _productRepository.GetAllAsync(cancellationToken);
        int inventoryItems = products.Count;

        var salesByCategoryRaw = await _salesRepository.GetSalesByCategoryAsync(cancellationToken);
        var salesByCategory = salesByCategoryRaw
            .Select(x => new SalesByCategoryDto(x.CategoryName, x.TotalSales))
            .ToList();
        
        var topCashiers = activeSales
            .GroupBy(x => x.CreatedByUserId)
            .Select(g => new TopCashierDto(
                CashierName: g.Key?.ToString() ?? "Unknown", 
                SalesCount: g.Count(), 
                TotalRevenue: g.Sum(x => x.GrandTotal)))
            .OrderByDescending(x => x.TotalRevenue)
            .Take(5)
            .ToList();

        var stockLevels = await _stockRepository.GetAllAsync(cancellationToken);
        var lowStockAlerts = stockLevels
            .Where(x => x.QuantityOnHand <= 10) // Mock threshold
            .Select(x => new LowStockAlertDto(
                ProductName: products.FirstOrDefault(p => p.Id == x.ProductId)?.Name ?? "Unknown Product",
                QuantityOnHand: x.QuantityOnHand,
                ReorderThreshold: 10))
            .Take(5)
            .ToList();

        return new DashboardMetricsDto(
            totalRevenue,
            activeOrders,
            totalCustomers,
            inventoryItems,
            salesByCategory,
            topCashiers,
            lowStockAlerts
        );
    }
}
