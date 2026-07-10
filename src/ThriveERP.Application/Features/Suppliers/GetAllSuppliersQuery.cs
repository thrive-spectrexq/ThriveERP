namespace ThriveERP.Application.Features.Suppliers;
using MediatR;
public record GetAllSuppliersQuery : IRequest<List<SupplierDto>>;
