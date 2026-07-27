using System;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Features.Returns;

public record ReturnDto : IMapFrom<Return>
{
    public Guid Id { get; init; }
    public Guid SalesOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal RefundAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime ProcessedAtUtc { get; init; }
}
