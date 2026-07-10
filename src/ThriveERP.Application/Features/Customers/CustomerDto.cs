namespace ThriveERP.Application.Features.Customers;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;
public record CustomerDto : IMapFrom<Customer>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal CurrentBalance { get; init; }
    public bool IsActive { get; init; }
}
