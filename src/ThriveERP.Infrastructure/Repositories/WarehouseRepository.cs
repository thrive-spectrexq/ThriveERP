namespace ThriveERP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Infrastructure.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(ThriveErpDbContext context) : base(context)
    {
    }
}
