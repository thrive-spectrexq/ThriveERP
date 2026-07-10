using System;
using MediatR;

namespace ThriveERP.Application.Features.Customers;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    decimal CreditLimit,
    bool IsActive) : IRequest<CustomerDto>;
