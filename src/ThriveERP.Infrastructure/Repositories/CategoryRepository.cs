namespace ThriveERP.Infrastructure.Repositories;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ThriveErpDbContext context) : base(context) { }
}
