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

    public void GenerateSalesOrderPdf(SalesOrder order, string outputPath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeHeader(c, order));
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

    private void ComposeHeader(IContainer container, SalesOrder order)
    {
        var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);

        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("ThriveERP Inc.").Style(titleStyle);
                column.Item().Text("123 Business Road");
                column.Item().Text("City, State, 12345");
                column.Item().Text("Email: support@thriveerp.com");
            });

            row.ConstantItem(100).Height(50).Placeholder(); // For Logo
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

            var totalPrice = order.GrandTotal;
            column.Item().PaddingRight(5).AlignRight().Text($"Total: {totalPrice:C}").SemiBold().FontSize(14);
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
