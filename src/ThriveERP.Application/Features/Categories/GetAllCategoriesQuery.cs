namespace ThriveERP.Application.Features.Categories;
using MediatR;
public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>;
