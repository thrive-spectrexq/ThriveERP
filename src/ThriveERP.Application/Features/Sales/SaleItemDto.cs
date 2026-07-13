using System;
using ThriveERP.Application.Common.Mappings;
using ThriveERP.Domain.Entities;
using AutoMapper;

namespace ThriveERP.Application.Features.Sales;

public record SaleItemDto : IMapFrom<SaleItem>
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal LineTotal { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SaleItem, SaleItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty));
    }
}
