namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Provides Excel spreadsheet export capabilities.
/// </summary>
public interface IExcelExportService
{
    /// <summary>Exports the given data as an Excel workbook written to <paramref name="stream"/>.</summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    /// <param name="stream">The output stream to write the workbook to.</param>
    /// <param name="data">The data to render in the spreadsheet.</param>
    /// <param name="ct">A cancellation token.</param>
    Task ExportAsync<T>(Stream stream, T data, CancellationToken ct = default);
}
