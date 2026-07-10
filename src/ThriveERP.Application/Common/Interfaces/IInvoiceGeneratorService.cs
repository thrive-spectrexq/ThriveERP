using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Common.Interfaces;

public interface IInvoiceGeneratorService
{
    void GenerateSalesOrderPdf(SalesOrder order, string outputPath);
}
