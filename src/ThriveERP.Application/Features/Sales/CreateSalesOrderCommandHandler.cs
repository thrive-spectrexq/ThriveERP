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
            warehouseId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = await _salesOrderRepository.GetNextOrderNumberAsync(),
            CustomerId = request.CustomerId,
            WarehouseId = warehouseId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Draft
        };

        foreach (var itemDto in request.Items)
        {
            // Use domain method AddItem() which enforces business rules
            // and recalculates totals automatically
            order.AddItem(new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                DiscountAmount = itemDto.DiscountAmount
            });

            // Use domain method AdjustQuantity() which enforces non-negative stock
            var stockLevel = await _stockLevelRepository.GetByProductAndWarehouseAsync(
                itemDto.ProductId, warehouseId, cancellationToken);
            if (stockLevel != null)
            {
                stockLevel.AdjustQuantity(-itemDto.Quantity);
                _stockLevelRepository.Update(stockLevel);
            }
        }

        // Use domain method Submit() which enforces state machine and raises domain events
        order.Submit();

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
