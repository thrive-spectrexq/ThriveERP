using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Application.Features.Sales;

public class ExportInvoiceCommandHandler : IRequestHandler<ExportInvoiceCommand, string>
{
    private readonly ISalesOrderRepository _repository;
    private readonly IInvoiceGeneratorService _invoiceGenerator;

    public ExportInvoiceCommandHandler(ISalesOrderRepository repository, IInvoiceGeneratorService invoiceGenerator)
    {
        _repository = repository;
        _invoiceGenerator = invoiceGenerator;
    }

    public async Task<string> Handle(ExportInvoiceCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetWithItemsAsync(request.SalesOrderId);
        if (order == null)
            throw new System.Exception("SalesOrder not found");

        if (!Directory.Exists(request.OutputDirectory))
        {
            Directory.CreateDirectory(request.OutputDirectory);
        }

        var fileName = $"Invoice_{order.OrderNumber}.pdf";
        var fullPath = Path.Combine(request.OutputDirectory, fileName);

        _invoiceGenerator.GenerateSalesOrderPdf(order, fullPath);

        return fullPath;
    }
}
