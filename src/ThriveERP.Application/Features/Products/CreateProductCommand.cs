namespace ThriveERP.Application.Features.Products;
using MediatR;
public record CreateProductCommand(
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
    bool IsActive
) : IRequest<ProductDto>;
