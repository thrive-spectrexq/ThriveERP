using System;
using MediatR;

namespace ThriveERP.Application.Features.Sales;

// Returns the file path of the generated PDF
public record ExportInvoiceCommand(Guid SalesOrderId, string OutputDirectory) : IRequest<string>;
