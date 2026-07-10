namespace ThriveERP.Application.Features.Sales;

using System;
using System.Collections.Generic;
using MediatR;

public record CreateSaleItemDto(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount
);

public record CreateSalesOrderCommand(
    Guid? CustomerId,
    Guid WarehouseId,
    List<CreateSaleItemDto> Items
) : IRequest<SalesOrderDto>;
