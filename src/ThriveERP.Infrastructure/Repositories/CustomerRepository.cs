namespace ThriveERP.Infrastructure.Repositories;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ThriveErpDbContext context) : base(context) { }
}
