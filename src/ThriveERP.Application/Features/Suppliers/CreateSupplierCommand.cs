namespace ThriveERP.Application.Features.Suppliers;
using MediatR;
public record CreateSupplierCommand(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    bool IsActive = true
) : IRequest<SupplierDto>;
