namespace ThriveERP.Application.Features.Customers;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;
public record CustomerDto : IMapFrom<Customer>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
