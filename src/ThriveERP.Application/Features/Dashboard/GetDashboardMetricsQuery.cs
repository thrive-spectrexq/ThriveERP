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
    private readonly IUserRepository _userRepository;

    public GetDashboardMetricsQueryHandler(
        ISalesOrderRepository salesRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IStockLevelRepository stockRepository,
        IUserRepository userRepository)
    {
        _salesRepository = salesRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _stockRepository = stockRepository;
        _userRepository = userRepository;
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
        
        // Resolve cashier names from UserRepository instead of showing GUIDs
        var allUsers = await _userRepository.GetAllAsync(cancellationToken);
        var userLookup = allUsers.ToDictionary(u => u.Id, u => u.FullName);

        var topCashiers = activeSales
            .GroupBy(x => x.CreatedByUserId)
            .Select(g => new TopCashierDto(
                CashierName: g.Key.HasValue && userLookup.TryGetValue(g.Key.Value, out var name) 
                    ? name 
                    : "Unknown Cashier", 
                SalesCount: g.Count(), 
                TotalRevenue: g.Sum(x => x.GrandTotal)))
            .OrderByDescending(x => x.TotalRevenue)
            .Take(5)
            .ToList();

        // Use actual Product.ReorderThreshold instead of hardcoded value
        var stockLevels = await _stockRepository.GetAllAsync(cancellationToken);
        var productLookup = products.ToDictionary(p => p.Id);

        var lowStockAlerts = stockLevels
            .Where(x =>
            {
                if (!productLookup.TryGetValue(x.ProductId, out var product)) return false;
                int threshold = product.ReorderThreshold > 0 ? product.ReorderThreshold : 10;
                return x.QuantityOnHand <= threshold;
            })
            .Select(x =>
            {
                var product = productLookup.GetValueOrDefault(x.ProductId);
                int threshold = product?.ReorderThreshold > 0 ? product.ReorderThreshold : 10;
                return new LowStockAlertDto(
                    ProductName: product?.Name ?? "Unknown Product",
                    QuantityOnHand: x.QuantityOnHand,
                    ReorderThreshold: threshold);
            })
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
