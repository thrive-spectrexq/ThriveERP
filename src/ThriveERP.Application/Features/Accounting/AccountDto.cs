namespace ThriveERP.Application.Features.Accounting;
using ThriveERP.Domain.Enums;
using System;
public record AccountDto(
    Guid Id,
    string Name,
    AccountType Type,
    decimal Balance,
    bool IsActive
);
