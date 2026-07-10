namespace ThriveERP.Infrastructure.Repositories;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ThriveErpDbContext context) : base(context)
    {
    }
}
