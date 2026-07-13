namespace ThriveERP.Application.Features.HR;
using MediatR;
using System;

public record CreateEmployeeCommand(
    string FullName,
    string? Position,
    string? Phone,
    string? Email,
    DateOnly? HireDate,
    bool CreateUserAccount,
    string? Username,
    string? Password,
    Guid? RoleId
) : IRequest<Guid>;
