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
    private readonly IStockLevelRepository _stockLevelRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSalesOrderCommandHandler(
        ISalesOrderRepository repository, 
        IStockLevelRepository stockLevelRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork, 
        IMapper mapper)
    {
        _salesOrderRepository = repository;
        _stockLevelRepository = stockLevelRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SalesOrderDto> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var warehouseId = request.WarehouseId;
        if (warehouseId == Guid.Empty)
        {
            warehouseId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // We will seed this
        }

        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = await _salesOrderRepository.GetNextOrderNumberAsync(),
            CustomerId = request.CustomerId,
            WarehouseId = warehouseId,
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

            // Deduct inventory
            var stockLevel = await _stockLevelRepository.GetByProductAndWarehouseAsync(itemDto.ProductId, warehouseId);
            if (stockLevel != null)
            {
                stockLevel.QuantityOnHand -= itemDto.Quantity;
                _stockLevelRepository.Update(stockLevel);
            }
        }

        order.Subtotal = subtotal;
        order.TaxTotal = 0; // Configurable tax later
        order.GrandTotal = subtotal + order.TaxTotal;

        // Auto-submit the order since it's from POS
        order.Status = ThriveERP.Domain.Enums.OrderStatus.Submitted;

        // Update customer balance if applicable
        if (order.CustomerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId.Value, cancellationToken);
            if (customer != null)
            {
                customer.CurrentBalance += order.GrandTotal;
                _customerRepository.Update(customer);
            }
        }

        await _salesOrderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<SalesOrderDto>(order);
    }
}
