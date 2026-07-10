namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Provides PDF document export capabilities.
/// </summary>
public interface IPdfExportService
{
    /// <summary>Exports the given data as a PDF document written to <paramref name="stream"/>.</summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    /// <param name="stream">The output stream to write the PDF to.</param>
    /// <param name="data">The data to render in the PDF.</param>
    /// <param name="ct">A cancellation token.</param>
    Task ExportAsync<T>(Stream stream, T data, CancellationToken ct = default);
}
