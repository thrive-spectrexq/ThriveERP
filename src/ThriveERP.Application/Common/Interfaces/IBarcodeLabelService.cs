using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Common.Interfaces;

public interface IBarcodeLabelService
{
    Task<string> GenerateLabelPdfAsync(Product product, string outputDirectory, CancellationToken ct = default);
}
