namespace ThriveERP.Application.Features.Purchasing;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePurchaseOrderCommandHandler(IPurchaseOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchaseOrderDto> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new PurchaseOrder
        {
            OrderNumber = "PO-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            Status = PurchaseOrderStatus.Draft,
            OrderDate = DateTime.UtcNow,
            ExpectedDate = request.ExpectedDate,
            PurchaseItems = request.Items.Select(i => new PurchaseItem
            {
                ProductId = i.ProductId,
                QuantityOrdered = i.QuantityOrdered,
                UnitCost = i.UnitCost,
                LineTotal = i.QuantityOrdered * i.UnitCost
            }).ToList()
        };

        order.Subtotal = order.PurchaseItems.Sum(x => x.LineTotal);
        order.TaxTotal = 0; // simplify for now
        order.GrandTotal = order.Subtotal + order.TaxTotal;

        await _repository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PurchaseOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            SupplierId = order.SupplierId,
            WarehouseId = order.WarehouseId,
            Status = order.Status.ToString(),
            Subtotal = order.Subtotal,
            TaxTotal = order.TaxTotal,
            GrandTotal = order.GrandTotal,
            OrderDate = order.OrderDate,
            ExpectedDate = order.ExpectedDate
        };
    }
}
