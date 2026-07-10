namespace ThriveERP.Application.Features.Products;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;
public record ProductDto : IMapFrom<Product>
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public Guid? BrandId { get; init; }
    public string? BrandName { get; init; }
    public decimal CostPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public bool TrackBatches { get; init; }
    public int ReorderThreshold { get; init; }
    public bool IsActive { get; init; }
}
