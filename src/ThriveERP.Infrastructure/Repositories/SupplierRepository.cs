namespace ThriveERP.Infrastructure.Repositories;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ThriveErpDbContext context) : base(context) { }
}
