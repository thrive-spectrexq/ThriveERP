namespace ThriveERP.Application.Features.Suppliers;
using MediatR;
using System;
public record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    bool IsActive = true
) : IRequest;
