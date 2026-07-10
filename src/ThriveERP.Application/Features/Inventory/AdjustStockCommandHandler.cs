using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Domain.Exceptions;

namespace ThriveERP.Application.Features.Inventory;

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, bool>
{
    private readonly IStockLevelRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AdjustStockCommandHandler(IStockLevelRepository repository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var stockLevel = await _repository.GetByProductAndWarehouseAsync(request.ProductId, request.WarehouseId);

        if (stockLevel == null)
        {
            stockLevel = new StockLevel
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                QuantityOnHand = 0
            };
            await _repository.AddAsync(stockLevel, cancellationToken);
        }

        stockLevel.AdjustQuantity(request.QuantityChange);

        _repository.Update(stockLevel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
