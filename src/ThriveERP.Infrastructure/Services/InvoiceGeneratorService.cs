using System;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;

namespace ThriveERP.Infrastructure.Services;

public class InvoiceGeneratorService : IInvoiceGeneratorService
{
    public InvoiceGeneratorService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void GenerateSalesOrderPdf(SalesOrder order, string outputPath, string businessName = "ThriveERP Inc.", string businessAddress = "123 Business Road", string businessPhone = "", string businessEmail = "support@thriveerp.com")
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeHeader(c, order, businessName, businessAddress, businessPhone, businessEmail));
                page.Content().Element(c => ComposeContent(c, order));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        })
        .GeneratePdf(outputPath);
    }

    private void ComposeHeader(IContainer container, SalesOrder order, string businessName, string businessAddress, string businessPhone, string businessEmail)
    {
        var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);

        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(businessName).Style(titleStyle);
                if (!string.IsNullOrWhiteSpace(businessAddress)) column.Item().Text(businessAddress);
                if (!string.IsNullOrWhiteSpace(businessPhone)) column.Item().Text($"Phone: {businessPhone}");
                if (!string.IsNullOrWhiteSpace(businessEmail)) column.Item().Text($"Email: {businessEmail}");
            });

            row.RelativeItem().AlignRight().Column(column =>
            {
                column.Item().Text($"Invoice #: INV-{order.OrderNumber}").SemiBold().FontSize(14);
            });
        });
    }

    private void ComposeContent(IContainer container, SalesOrder order)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(20);

            column.Item().Row(row =>
            {
                row.RelativeItem().Component(new AddressComponent("Bill To", order.Customer?.Name ?? "Walk-in Customer", order.Customer?.Address ?? ""));
                row.ConstantItem(50);
                row.RelativeItem().Component(new OrderDetailsComponent(order));
            });

            column.Item().Element(c => ComposeTable(c, order));

            column.Item().PaddingRight(5).AlignRight().Column(totals =>
            {
                totals.Spacing(2);
                totals.Item().Text($"Subtotal: {order.Subtotal:C}").FontSize(12);
                if (order.DiscountTotal > 0)
                {
                    totals.Item().Text($"Discount: -{order.DiscountTotal:C}").FontSize(12);
                }
                totals.Item().Text($"Tax: {order.TaxTotal:C}").FontSize(12);
                totals.Item().Text($"Grand Total: {order.GrandTotal:C}").SemiBold().FontSize(14);
            });
        });
    }

    private void ComposeTable(IContainer container, SalesOrder order)
    {
        container.Table(table =>
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
                header.Cell().Element(CellStyle).Text("#");
                header.Cell().Element(CellStyle).Text("Item");
                header.Cell().Element(CellStyle).AlignRight().Text("Unit Price");
                header.Cell().Element(CellStyle).AlignRight().Text("Quantity");
                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            if (order.SaleItems != null)
            {
                int index = 1;
                foreach (var item in order.SaleItems)
                {
                    table.Cell().Element(CellStyle).Text(index.ToString());
                    table.Cell().Element(CellStyle).Text(item.Product?.Name ?? $"Product {item.ProductId}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:C}");
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.LineTotal:C}");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                    index++;
                }
            }
        });
    }
}

public class AddressComponent : IComponent
{
    private string Title { get; }
    private string Name { get; }
    private string Address { get; }

    public AddressComponent(string title, string name, string address)
    {
        Title = title;
        Name = name;
        Address = address;
    }

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(2);
            column.Item().Text(Title).SemiBold();
            column.Item().PaddingBottom(5).LineHorizontal(1);
            column.Item().Text(Name);
            column.Item().Text(Address);
        });
    }
}

public class OrderDetailsComponent : IComponent
{
    private SalesOrder Order { get; }

    public OrderDetailsComponent(SalesOrder order)
    {
        Order = order;
    }

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(2);
            column.Item().Text("Order Details").SemiBold();
            column.Item().PaddingBottom(5).LineHorizontal(1);
            column.Item().Text($"Order #: {Order.OrderNumber}");
            column.Item().Text($"Date: {Order.OrderDate:d}");
            column.Item().Text($"Status: {Order.Status}");
        });
    }
}
