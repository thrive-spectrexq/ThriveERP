using System;
using MediatR;

namespace ThriveERP.Application.Features.Customers;

public record DeleteCustomerCommand(Guid Id) : IRequest<bool>;
