namespace ThriveERP.Application.Features.Sales;

using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Domain.Enums;

public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, SalesOrderDto>
{
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSalesOrderCommandHandler(ISalesOrderRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _salesOrderRepository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SalesOrderDto> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var orderNumber = await _salesOrderRepository.GetNextOrderNumberAsync();
        
        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            OrderDate = DateTime.UtcNow,
            Status = ThriveERP.Domain.Enums.OrderStatus.Draft
        };

        decimal subtotal = 0;
        foreach (var itemDto in request.Items)
        {
            var lineTotal = (itemDto.Quantity * itemDto.UnitPrice) - itemDto.DiscountAmount;
            subtotal += lineTotal;

            order.SaleItems.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                SalesOrderId = order.Id,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                DiscountAmount = itemDto.DiscountAmount,
                LineTotal = lineTotal
            });
        }

        order.Subtotal = subtotal;
        order.TaxTotal = 0; // Configurable tax later
        order.GrandTotal = subtotal + order.TaxTotal;

        await _salesOrderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<SalesOrderDto>(order);
    }
}
