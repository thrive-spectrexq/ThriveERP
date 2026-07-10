namespace ThriveERP.Application.Features.Suppliers;
using MediatR;
using System;
public record DeleteSupplierCommand(Guid Id) : IRequest;
