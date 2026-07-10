using NetBarcode;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;

namespace ThriveERP.Infrastructure.Services;

public class BarcodeLabelService : IBarcodeLabelService
{
    public BarcodeLabelService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateLabelPdfAsync(Product product, string outputDirectory, CancellationToken ct = default)
    {
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string fileName = $"Label_{product.Sku}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string filePath = Path.Combine(outputDirectory, fileName);

        // Generate Barcode Image using NetBarcode
        var barcode = new Barcode(product.Barcode ?? product.Sku, NetBarcode.Type.Code128, true);
        var base64Image = barcode.GetBase64Image();
        byte[] imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", ""));

        // Generate PDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Typical label size (e.g., 2x1 inches)
                page.Size(new PageSize(2 * 72, 1 * 72));
                page.Margin(5);
                page.PageColor(Colors.White);

                page.Content()
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text(product.Name).FontSize(10).SemiBold();
                        col.Item().AlignCenter().Text($"SKU: {product.Sku}").FontSize(8);
                        col.Item().AlignCenter().Image(imageBytes).FitHeight();
                        col.Item().AlignCenter().Text($"${product.SellingPrice:F2}").FontSize(10).Bold();
                    });
            });
        });

        document.GeneratePdf(filePath);
        return await Task.FromResult(filePath);
    }
}
