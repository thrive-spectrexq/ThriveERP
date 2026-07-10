namespace ThriveERP.Application.Features.Customers;
using MediatR;
public record GetAllCustomersQuery : IRequest<List<CustomerDto>>;
