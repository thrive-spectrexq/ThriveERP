namespace ThriveERP.Application.Features.Sales;

using System;
using MediatR;

public record CreateSalesOrderCommand(
    Guid? CustomerId,
    Guid WarehouseId,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal
) : IRequest<SalesOrderDto>;
