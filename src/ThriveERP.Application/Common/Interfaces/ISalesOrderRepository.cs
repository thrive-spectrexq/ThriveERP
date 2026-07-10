using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Repository contract for <see cref="SalesOrder"/> entities.
/// </summary>
public interface ISalesOrderRepository : IRepository<SalesOrder>
{
    /// <summary>Gets a sales order eagerly loading its line items.</summary>
    Task<SalesOrder?> GetWithItemsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Generates the next sequential order number.</summary>
    Task<string> GetNextOrderNumberAsync(CancellationToken ct = default);
}
