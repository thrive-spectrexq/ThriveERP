namespace ThriveERP.Application.Features.Accounting;
using ThriveERP.Domain.Enums;
using System;
public record TransactionDto(
    Guid Id,
    Guid AccountId,
    decimal Amount,
    TransactionType Type,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Description,
    DateTime OccurredAtUtc
);
