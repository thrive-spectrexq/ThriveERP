using System;
using MediatR;
using ThriveERP.Domain.Enums;

namespace ThriveERP.Application.Features.Inventory;

public record AdjustStockCommand(
    Guid ProductId,
    Guid WarehouseId,
    decimal QuantityChange,
    MovementType MovementType,
    string? Reason) : IRequest<bool>;
