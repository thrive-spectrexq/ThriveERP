namespace ThriveERP.Application.Features.Categories;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;
public record CategoryDto : IMapFrom<Category>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
