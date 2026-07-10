namespace ThriveERP.Application.Features.Suppliers;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;
public record SupplierDto : IMapFrom<Supplier>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
