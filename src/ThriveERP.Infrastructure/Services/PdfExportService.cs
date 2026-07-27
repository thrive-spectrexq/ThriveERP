using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Application.Features.Returns;
using ThriveERP.Application.Features.Sales;

namespace ThriveERP.Infrastructure.Services;

public class PdfExportService : IPdfExportService
{
    public PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task ExportAsync<T>(Stream stream, T data, CancellationToken ct = default)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));
                
                page.Header()
                    .Text("ThriveERP Report Document")
                    .SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);
                
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);
                        x.Item().Text("Document Details:");
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

    public Task ExportReceiptAsync(
        Stream stream, 
        SalesOrderDto order, 
        string businessName, 
        string? businessPhone = null, 
        string? businessAddress = null, 
        string? footerNote = null, 
        CancellationToken ct = default)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));
                
                // Header (Clean Business Info - No Placeholder Image / Logo Box)
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(businessName).Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            if (!string.IsNullOrWhiteSpace(businessAddress))
                            {
                                c.Item().Text(businessAddress).FontSize(10).FontColor(Colors.Grey.Darken2);
                            }
                            if (!string.IsNullOrWhiteSpace(businessPhone))
                            {
                                c.Item().Text($"Phone: {businessPhone}").FontSize(10).FontColor(Colors.Grey.Darken2);
                            }
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("RECEIPT").Bold().FontSize(22).FontColor(Colors.Grey.Darken3);
                            c.Item().Text($"Receipt #: {order.OrderNumber}").FontSize(10).SemiBold();
                            c.Item().Text($"Date: {order.OrderDate:g}").FontSize(10).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // Content (Items Table & Totals formatted like Invoice PDF)
                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(15);

                    // Line Items Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("#");
                            header.Cell().Element(HeaderStyle).Text("Item Description");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Unit Price");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Qty");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Line Total");

                            static IContainer HeaderStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold().FontSize(10))
                                                .PaddingVertical(6)
                                                .BorderBottom(1.5f)
                                                .BorderColor(Colors.Grey.Darken1);
                            }
                        });

                        int index = 1;
                        foreach (var item in order.Items)
                        {
                            table.Cell().Element(RowStyle).Text(index.ToString());
                            table.Cell().Element(RowStyle).Text(item.ProductName);
                            table.Cell().Element(RowStyle).AlignRight().Text($"{item.UnitPrice:C}");
                            table.Cell().Element(RowStyle).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().Element(RowStyle).AlignRight().Text($"{item.LineTotal:C}");

                            static IContainer RowStyle(IContainer container)
                            {
                                return container.BorderBottom(1)
                                                .BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(6);
                            }
                            index++;
                        }
                    });

                    // Summary / Totals Box
                    col.Item().AlignRight().Column(totals =>
                    {
                        totals.Spacing(4);
                        totals.Item().Text($"Subtotal: {order.Subtotal:C}").FontSize(10);
                        if (order.DiscountTotal > 0)
                        {
                            totals.Item().Text($"Discount Savings: -{order.DiscountTotal:C}").FontSize(10).FontColor(Colors.Green.Darken2);
                        }
                        totals.Item().Text($"Tax (10%): {order.TaxTotal:C}").FontSize(10);
                        totals.Item().PaddingTop(4).Text($"Grand Total: {order.GrandTotal:C}").Bold().FontSize(15).FontColor(Colors.Blue.Darken2);
                    });
                });

                // Footer
                page.Footer().Column(f =>
                {
                    f.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    string footerMsg = !string.IsNullOrWhiteSpace(footerNote) ? footerNote : "Thank you for your business!";
                    f.Item().PaddingTop(6).AlignCenter().Text(footerMsg).Italic().FontSize(10).FontColor(Colors.Grey.Darken2);
                });
            });
        })
        .GeneratePdf(stream);
        
        return Task.CompletedTask;
    }

    public Task ExportCreditNoteAsync(
        Stream stream, 
        ReturnDto returnData, 
        string businessName, 
        string? businessPhone = null, 
        string? businessAddress = null, 
        CancellationToken ct = default)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));
                
                // Header
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(businessName).Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            if (!string.IsNullOrWhiteSpace(businessAddress))
                            {
                                c.Item().Text(businessAddress).FontSize(10).FontColor(Colors.Grey.Darken2);
                            }
                            if (!string.IsNullOrWhiteSpace(businessPhone))
                            {
                                c.Item().Text($"Phone: {businessPhone}").FontSize(10).FontColor(Colors.Grey.Darken2);
                            }
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("REFUND CREDIT NOTE").Bold().FontSize(20).FontColor(Colors.Red.Medium);
                            c.Item().Text($"Order #: {returnData.OrderNumber}").FontSize(10).SemiBold();
                            c.Item().Text($"Return ID: {returnData.Id.ToString().Substring(0, 8)}").FontSize(10);
                            c.Item().Text($"Date: {returnData.ProcessedAtUtc:g}").FontSize(10).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // Content
                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(15);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("#");
                            header.Cell().Element(HeaderStyle).Text("Returned Product");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Qty Returned");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Refunded Amount");

                            static IContainer HeaderStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold().FontSize(10))
                                                .PaddingVertical(6)
                                                .BorderBottom(1.5f)
                                                .BorderColor(Colors.Red.Darken1);
                            }
                        });

                        table.Cell().Element(RowStyle).Text("1");
                        table.Cell().Element(RowStyle).Text(returnData.ProductName);
                        table.Cell().Element(RowStyle).AlignRight().Text(returnData.Quantity.ToString());
                        table.Cell().Element(RowStyle).AlignRight().Text($"-{returnData.RefundAmount:C}");

                        static IContainer RowStyle(IContainer container)
                        {
                            return container.BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(6);
                        }
                    });

                    col.Item().AlignRight().Column(totals =>
                    {
                        totals.Spacing(4);
                        totals.Item().Text($"Return Reason: {returnData.Reason}").FontSize(10).Italic();
                        totals.Item().PaddingTop(4).Text($"TOTAL REFUND: -{returnData.RefundAmount:C}").Bold().FontSize(15).FontColor(Colors.Red.Darken1);
                    });
                });

                // Footer
                page.Footer().Column(f =>
                {
                    f.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    f.Item().PaddingTop(6).AlignCenter().Text("Official Customer Refund Slip").Italic().FontSize(10).FontColor(Colors.Grey.Darken2);
                });
            });
        })
        .GeneratePdf(stream);
        
        return Task.CompletedTask;
    }
}
