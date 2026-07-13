namespace ThriveERP.Application.Features.Sales;

using System;
using MediatR;

public record GetSalesOrderByIdQuery(Guid Id) : IRequest<SalesOrderDto?>;
