namespace ThriveERP.Application.Features.Products;
using MediatR;
public record GetAllProductsQuery : IRequest<List<ProductDto>>;
