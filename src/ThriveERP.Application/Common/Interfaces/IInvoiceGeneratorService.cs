using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Common.Interfaces;

public interface IInvoiceGeneratorService
{
    void GenerateSalesOrderPdf(SalesOrder order, string outputPath, string businessName = "ThriveERP Inc.", string businessAddress = "123 Business Road", string businessPhone = "", string businessEmail = "support@thriveerp.com");
}
