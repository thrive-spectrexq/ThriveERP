using System;
using MediatR;

namespace ThriveERP.Application.Features.Products;

public record UpdateProductCommand(
    Guid Id,
    string Sku,
    string? Barcode,
    string Name,
    string? Description,
    Guid? CategoryId,
    Guid? BrandId,
    decimal CostPrice,
    decimal SellingPrice,
    bool TrackBatches,
    int ReorderThreshold,
    bool IsActive) : IRequest<ProductDto>;
