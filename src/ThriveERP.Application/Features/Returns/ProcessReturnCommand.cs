namespace ThriveERP.Application.Features.Returns;

using MediatR;
using System;

public record ProcessReturnCommand(
    Guid SalesOrderId,
    Guid ProductId,
    decimal Quantity,
    string? Reason) : IRequest<bool>;
