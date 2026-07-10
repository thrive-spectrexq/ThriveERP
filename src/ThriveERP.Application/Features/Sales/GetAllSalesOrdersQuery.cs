namespace ThriveERP.Application.Features.Sales;

using System.Collections.Generic;
using MediatR;

public record GetAllSalesOrdersQuery : IRequest<List<SalesOrderDto>>;
