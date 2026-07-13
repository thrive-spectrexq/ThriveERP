using System;
using System.Collections.Generic;

namespace ThriveERP.Application.Features.Reports;

public record DailySalesReportDto
{
    public DateTime Date { get; init; }
    public decimal TotalSales { get; init; }
    public int OrderCount { get; init; }
    public List<SaleItemSummaryDto> TopSellingItems { get; init; } = new();
}

public record SaleItemSummaryDto(string ProductName, decimal QuantitySold, decimal Revenue);
