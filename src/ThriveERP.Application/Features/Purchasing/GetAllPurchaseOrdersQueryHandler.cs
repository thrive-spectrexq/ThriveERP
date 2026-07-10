namespace ThriveERP.Application.Features.Purchasing;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetAllPurchaseOrdersQueryHandler : IRequestHandler<GetAllPurchaseOrdersQuery, List<PurchaseOrderDto>>
{
    private readonly IPurchaseOrderRepository _repository;

    public GetAllPurchaseOrdersQueryHandler(IPurchaseOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PurchaseOrderDto>> Handle(GetAllPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetAllAsync(cancellationToken);
        return orders.Select(o => new PurchaseOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            SupplierId = o.SupplierId,
            SupplierName = o.Supplier?.Name ?? "Unknown Supplier",
            WarehouseId = o.WarehouseId,
            WarehouseName = o.Warehouse?.Name ?? "Unknown Warehouse",
            Status = o.Status.ToString(),
            Subtotal = o.Subtotal,
            TaxTotal = o.TaxTotal,
            GrandTotal = o.GrandTotal,
            OrderDate = o.OrderDate,
            ExpectedDate = o.ExpectedDate
        }).ToList();
    }
}
