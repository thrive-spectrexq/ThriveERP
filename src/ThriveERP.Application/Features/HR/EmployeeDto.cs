namespace ThriveERP.Application.Features.HR;
using System;

public record EmployeeDto(
    Guid Id,
    Guid? UserId,
    string FullName,
    string? Position,
    string? Phone,
    string? Email,
    DateOnly? HireDate,
    bool IsActive
);
