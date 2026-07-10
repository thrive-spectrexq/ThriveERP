namespace ThriveERP.Application.Features.Purchasing;
using MediatR;
using System.Collections.Generic;

public record GetAllPurchaseOrdersQuery : IRequest<List<PurchaseOrderDto>>;
