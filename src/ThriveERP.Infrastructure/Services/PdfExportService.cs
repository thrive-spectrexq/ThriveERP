using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Infrastructure.Services;

public class PdfExportService : IPdfExportService
{
    public PdfExportService()
    {
        // QuestPDF requires setting the license type for Community users
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task ExportAsync<T>(Stream stream, T data, CancellationToken ct = default)
    {
        // For a full implementation, you would check `data is MySpecificReportData` 
        // and create the corresponding IDocument layout.
        
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));
                
                page.Header()
                    .Text("ThriveERP Report")
                    .SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);
                
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);
                        
                        x.Item().Text("Report Data:");
                        x.Item().Text(data?.ToString() ?? "No data provided.");
                    });
                
                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        })
        .GeneratePdf(stream);
        
        return Task.CompletedTask;
    }
}
