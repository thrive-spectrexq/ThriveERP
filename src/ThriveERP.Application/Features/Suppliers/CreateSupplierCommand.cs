namespace ThriveERP.Application.Features.Suppliers;
using MediatR;
public record CreateSupplierCommand(string Name) : IRequest<SupplierDto>;
