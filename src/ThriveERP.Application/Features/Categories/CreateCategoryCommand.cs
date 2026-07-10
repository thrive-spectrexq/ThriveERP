namespace ThriveERP.Application.Features.Categories;
using MediatR;
public record CreateCategoryCommand(string Name) : IRequest<CategoryDto>;
