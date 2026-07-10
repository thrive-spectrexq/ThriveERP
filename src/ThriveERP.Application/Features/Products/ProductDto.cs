namespace ThriveERP.Application.Features.Products;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;
public record ProductDto : IMapFrom<Product>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
