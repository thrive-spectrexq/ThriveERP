namespace ThriveERP.Application.Features.Suppliers;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;

public record SupplierDto : IMapFrom<Supplier>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public decimal CurrentBalance { get; init; }
    public bool IsActive { get; init; }
}
