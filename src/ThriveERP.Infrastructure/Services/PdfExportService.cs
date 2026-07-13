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

    public Task ExportReceiptAsync(Stream stream, ThriveERP.Application.Features.Sales.SalesOrderDto order, string businessName, CancellationToken ct = default)
    {
        Document.Create(container =>
        {
            // 80mm thermal receipt roll (approx 3.14 inches)
            container.Page(page =>
            {
                page.Margin(5, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.ContinuousSize(80, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));
                
                page.Content()
                    .Column(x =>
                    {
                        // Header
                        x.Item().AlignCenter().Text(businessName).Bold().FontSize(14);
                        x.Item().AlignCenter().Text("Receipt").FontSize(12);
                        x.Item().PaddingTop(5).Text($"Order #: {order.OrderNumber}");
                        x.Item().Text($"Date: {order.OrderDate:g}");
                        x.Item().Text("--------------------------------");
                        
                        // Items
                        foreach (var item in order.Items)
                        {
                            x.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{item.Quantity}x {item.ProductName}");
                                r.ConstantItem(50).AlignRight().Text(item.LineTotal.ToString("C"));
                            });
                        }
                        
                        x.Item().Text("--------------------------------");
                        
                        // Totals
                        x.Item().AlignRight().Text($"Subtotal: {order.Subtotal:C}");
                        if (order.DiscountTotal > 0) x.Item().AlignRight().Text($"Discount: {order.DiscountTotal:C}");
                        x.Item().AlignRight().Text($"Tax: {order.TaxTotal:C}");
                        x.Item().AlignRight().Text($"Total: {order.GrandTotal:C}").Bold().FontSize(12);
                        
                        // Footer
                        x.Item().PaddingTop(10).AlignCenter().Text("Thank you for your business!").Italic();
                    });
            });
        })
        .GeneratePdf(stream);
        
        return Task.CompletedTask;
    }
}
