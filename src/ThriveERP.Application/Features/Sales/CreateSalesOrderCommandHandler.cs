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
    private readonly IRepository<SalesOrder> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSalesOrderCommandHandler(IRepository<SalesOrder> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SalesOrderDto> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var entity = new SalesOrder 
        { 
            OrderNumber = "SO-" + DateTime.UtcNow.Ticks.ToString().Substring(10),
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            Status = OrderStatus.Draft,
            Subtotal = request.Subtotal,
            TaxTotal = request.TaxTotal,
            GrandTotal = request.GrandTotal,
            OrderDate = DateTime.UtcNow
        };
        
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<SalesOrderDto>(entity);
    }
}
