namespace ThriveERP.Application.Features.Customers;
using MediatR;
public record CreateCustomerCommand(string Name) : IRequest<CustomerDto>;
