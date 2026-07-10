using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Application.Features.Inventory;

public class GetStockLevelsQueryHandler : IRequestHandler<GetStockLevelsQuery, IReadOnlyList<StockLevelDto>>
{
    private readonly IStockLevelRepository _repository;
    private readonly IMapper _mapper;

    public GetStockLevelsQueryHandler(IStockLevelRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<StockLevelDto>> Handle(GetStockLevelsQuery request, CancellationToken cancellationToken)
    {
        var allLevels = await _repository.GetAllAsync(cancellationToken);
        
        var query = allLevels.AsQueryable();
        if (request.ProductId.HasValue)
            query = query.Where(x => x.ProductId == request.ProductId.Value);
        if (request.WarehouseId.HasValue)
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);

        var list = query.ToList();
        return list.Select(x => new StockLevelDto
        {
            Id = x.Id,
            ProductId = x.ProductId,
            ProductName = x.Product != null ? x.Product.Name : "Unknown Product",
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse != null ? x.Warehouse.Name : "Unknown Warehouse",
            QuantityOnHand = x.QuantityOnHand
        }).ToList();
    }
}
