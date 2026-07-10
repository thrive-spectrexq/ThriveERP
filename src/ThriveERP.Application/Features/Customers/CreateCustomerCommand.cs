namespace ThriveERP.Application.Features.Customers;
using MediatR;
public record CreateCustomerCommand(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    decimal CreditLimit,
    bool IsActive
) : IRequest<CustomerDto>;
