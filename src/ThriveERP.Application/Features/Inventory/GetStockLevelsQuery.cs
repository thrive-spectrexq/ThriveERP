using System;
using System.Collections.Generic;
using MediatR;

namespace ThriveERP.Application.Features.Inventory;

public record GetStockLevelsQuery(Guid? ProductId, Guid? WarehouseId) : IRequest<IReadOnlyList<StockLevelDto>>;
