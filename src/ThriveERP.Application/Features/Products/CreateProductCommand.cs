namespace ThriveERP.Application.Features.Products;
using MediatR;
public record CreateProductCommand(string Name) : IRequest<ProductDto>;
