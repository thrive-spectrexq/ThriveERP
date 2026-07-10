namespace ThriveERP.Application.Features.Accounting;
using System;
public record ExpenseDto(
    Guid Id,
    string Category,
    decimal Amount,
    Guid AccountId,
    string? Description,
    DateOnly ExpenseDate,
    Guid RecordedByUserId
);
