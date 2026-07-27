using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Features.Returns;
using ThriveERP.Application.Features.Sales;

namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Provides PDF document export capabilities.
/// </summary>
public interface IPdfExportService
{
    Task ExportAsync<T>(Stream stream, T data, CancellationToken ct = default);
    Task ExportReceiptAsync(Stream stream, SalesOrderDto order, string businessName, string? businessPhone = null, string? businessAddress = null, string? footerNote = null, CancellationToken ct = default);
    Task ExportCreditNoteAsync(Stream stream, ReturnDto returnData, string businessName, string? businessPhone = null, string? businessAddress = null, CancellationToken ct = default);
}
