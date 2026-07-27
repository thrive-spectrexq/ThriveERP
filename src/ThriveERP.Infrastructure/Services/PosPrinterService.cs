using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Application.Features.Sales;

namespace ThriveERP.Infrastructure.Services;

public class PosPrinterService : IPosPrinterService
{
    private readonly IPdfExportService _pdfExportService;

    public PosPrinterService(IPdfExportService pdfExportService)
    {
        _pdfExportService = pdfExportService;
    }

#pragma warning disable CA1416
    public List<string> GetInstalledPrinters()
    {
        var list = new List<string> { "Default System Printer", "PDF Document (Save to Disk)" };

        if (OperatingSystem.IsWindows())
        {
            try
            {
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (!list.Contains(printer))
                    {
                        list.Add(printer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enumerating printers: {ex.Message}");
            }
        }

        return list;
    }
#pragma warning restore CA1416

    public async Task<bool> PrintReceiptAsync(
        SalesOrderDto order,
        string printerName,
        string paperSize = "80mm",
        bool autoCut = true,
        bool openCashDrawer = true,
        string businessName = "ThriveERP",
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(printerName) || printerName.Contains("PDF"))
        {
            return await FallbackPdfExportAsync(order, businessName, ct);
        }

        string targetPrinter = resolvePrinterName(printerName);
        byte[] escPosBytes = BuildEscPosReceipt(order, paperSize, autoCut, openCashDrawer, businessName);

        // Try direct Windows Raw Spooler printing (winspool.drv)
        bool success = SendRawBytesToPrinter(targetPrinter, escPosBytes, $"ThriveERP Receipt #{order.OrderNumber}");

        if (!success)
        {
            // Fallback to PDF / System printing if Raw Spooling is not supported by driver
            return await FallbackPdfExportAsync(order, businessName, ct);
        }

        return true;
    }

    public async Task<bool> PrintTestReceiptAsync(
        string printerName,
        string paperSize = "80mm",
        string businessName = "ThriveERP",
        CancellationToken ct = default)
    {
        string targetPrinter = resolvePrinterName(printerName);

        var testOrder = new SalesOrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = "TEST-0001",
            OrderDate = DateTime.Now,
            Subtotal = 25.00m,
            TaxTotal = 2.50m,
            DiscountTotal = 0m,
            GrandTotal = 27.50m,
            Status = "Completed",
            Items = new List<SaleItemDto>
            {
                new SaleItemDto 
                { 
                    Id = Guid.NewGuid(), 
                    ProductId = Guid.NewGuid(), 
                    ProductName = "POS Printer Test Item", 
                    Quantity = 1, 
                    UnitPrice = 25.00m, 
                    LineTotal = 25.00m 
                }
            }
        };

        if (string.IsNullOrWhiteSpace(printerName) || printerName.Contains("PDF"))
        {
            return await FallbackPdfExportAsync(testOrder, $"{businessName} [TEST]", ct);
        }

        byte[] escPosBytes = BuildEscPosReceipt(testOrder, paperSize, true, true, $"{businessName} [TEST]");
        return SendRawBytesToPrinter(targetPrinter, escPosBytes, "ThriveERP Printer Test");
    }

#pragma warning disable CA1416
    private string resolvePrinterName(string printerName)
    {
        if (printerName == "Default System Printer" || string.IsNullOrWhiteSpace(printerName))
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    var settings = new PrinterSettings();
                    return settings.PrinterName;
                }
                catch
                {
                    return string.Empty;
                }
            }
            return string.Empty;
        }
        return printerName;
    }
#pragma warning restore CA1416

    private async Task<bool> FallbackPdfExportAsync(SalesOrderDto order, string businessName, CancellationToken ct)
    {
        try
        {
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var receiptPath = Path.Combine(downloadsPath, $"Receipt_{order.OrderNumber}.pdf");
            
            using (var stream = File.Create(receiptPath))
            {
                await _pdfExportService.ExportReceiptAsync(stream, order, businessName, ct: ct);
                await stream.FlushAsync(ct);
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = receiptPath,
                UseShellExecute = true
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fallback PDF export failed: {ex.Message}");
            return false;
        }
    }

    private byte[] BuildEscPosReceipt(SalesOrderDto order, string paperSize, bool autoCut, bool openCashDrawer, string businessName)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8);

        int cols = paperSize == "58mm" ? 32 : 48;

        // --- ESC/POS COMMAND CODES ---
        byte[] ESC_INIT = new byte[] { 0x1B, 0x40 }; // Initialize printer
        byte[] ESC_ALIGN_CENTER = new byte[] { 0x1B, 0x61, 0x01 };
        byte[] ESC_ALIGN_LEFT = new byte[] { 0x1B, 0x61, 0x00 };
        byte[] ESC_BOLD_ON = new byte[] { 0x1B, 0x45, 0x01 };
        byte[] ESC_BOLD_OFF = new byte[] { 0x1B, 0x45, 0x00 };
        byte[] ESC_DOUBLE_HEIGHT_ON = new byte[] { 0x1D, 0x21, 0x11 };
        byte[] ESC_TEXT_NORMAL = new byte[] { 0x1D, 0x21, 0x00 };
        byte[] ESC_CASH_DRAWER = new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA }; // Pulse pin 2
        byte[] ESC_PAPER_CUT = new byte[] { 0x1D, 0x56, 0x00 }; // Full cut

        // Open Cash Drawer if enabled
        if (openCashDrawer)
        {
            writer.Write(ESC_CASH_DRAWER);
        }

        // Initialize
        writer.Write(ESC_INIT);

        // Header (Business Name)
        writer.Write(ESC_ALIGN_CENTER);
        writer.Write(ESC_DOUBLE_HEIGHT_ON);
        writer.Write(Encoding.UTF8.GetBytes(businessName + "\n"));
        writer.Write(ESC_TEXT_NORMAL);
        writer.Write(Encoding.UTF8.GetBytes("OFFICIAL RECEIPT\n"));
        writer.Write(Encoding.UTF8.GetBytes(new string('-', cols) + "\n"));

        // Metadata
        writer.Write(ESC_ALIGN_LEFT);
        writer.Write(Encoding.UTF8.GetBytes($"Order #: {order.OrderNumber}\n"));
        writer.Write(Encoding.UTF8.GetBytes($"Date:   {order.OrderDate:g}\n"));
        writer.Write(Encoding.UTF8.GetBytes(new string('-', cols) + "\n"));

        // Items Header
        writer.Write(ESC_BOLD_ON);
        writer.Write(Encoding.UTF8.GetBytes(FormatLine("Item", "Total", cols) + "\n"));
        writer.Write(ESC_BOLD_OFF);
        writer.Write(Encoding.UTF8.GetBytes(new string('-', cols) + "\n"));

        // Items List
        foreach (var item in order.Items)
        {
            string itemDesc = $"{item.Quantity}x {item.ProductName}";
            string priceStr = item.LineTotal.ToString("C");
            
            if (itemDesc.Length > cols - priceStr.Length - 1)
            {
                itemDesc = itemDesc.Substring(0, cols - priceStr.Length - 4) + "...";
            }
            
            writer.Write(Encoding.UTF8.GetBytes(FormatLine(itemDesc, priceStr, cols) + "\n"));
        }

        writer.Write(Encoding.UTF8.GetBytes(new string('-', cols) + "\n"));

        // Totals Section
        writer.Write(Encoding.UTF8.GetBytes(FormatLine("Subtotal:", order.Subtotal.ToString("C"), cols) + "\n"));
        if (order.DiscountTotal > 0)
        {
            writer.Write(Encoding.UTF8.GetBytes(FormatLine("Discount:", $"-{order.DiscountTotal:C}", cols) + "\n"));
        }
        writer.Write(Encoding.UTF8.GetBytes(FormatLine("Tax (10%):", order.TaxTotal.ToString("C"), cols) + "\n"));
        
        writer.Write(ESC_BOLD_ON);
        writer.Write(Encoding.UTF8.GetBytes(FormatLine("TOTAL DUE:", order.GrandTotal.ToString("C"), cols) + "\n"));
        writer.Write(ESC_BOLD_OFF);
        writer.Write(Encoding.UTF8.GetBytes(new string('=', cols) + "\n"));

        // Footer Note
        writer.Write(ESC_ALIGN_CENTER);
        writer.Write(Encoding.UTF8.GetBytes("Thank you for shopping with us!\n"));
        writer.Write(Encoding.UTF8.GetBytes("ThriveERP - Enterprise POS System\n\n\n\n"));

        // Cut Paper if enabled
        if (autoCut)
        {
            writer.Write(ESC_PAPER_CUT);
        }

        return ms.ToArray();
    }

    private string FormatLine(string left, string right, int width)
    {
        int space = width - left.Length - right.Length;
        if (space < 1) space = 1;
        return left + new string(' ', space) + right;
    }

    // --- Direct Windows Spooler Native Raw Printing (winspool.drv) ---
    private bool SendRawBytesToPrinter(string szPrinterName, byte[] pBytes, string docName)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Direct raw spooling is supported on Windows host computers.");
            return false;
        }

        IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(pBytes.Length);
        Marshal.Copy(pBytes, 0, pUnmanagedBytes, pBytes.Length);

        bool success = false;
        IntPtr hPrinter = IntPtr.Zero;
        var di = new DOCINFOA
        {
            pDocName = docName,
            pDataType = "RAW"
        };

        try
        {
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        success = WritePrinter(hPrinter, pUnmanagedBytes, pBytes.Length, out _);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RawPrinterHelper Winspool error: {ex.Message}");
            success = false;
        }
        finally
        {
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
        }

        return success;
    }

    #region Win32 P/Invoke
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)] public string pDocName = "RAW Document";
        [MarshalAs(UnmanagedType.LPStr)] public string? pOutputFile = null;
        [MarshalAs(UnmanagedType.LPStr)] public string pDataType = "RAW";
    }

    [DllImport("winspool.drv", EntryPoint = "OpenPrinterA", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinterName, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.drv", EntryPoint = "ClosePrinter", ExactSpelling = true, SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "StartDocPrinterA", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA di);

    [DllImport("winspool.drv", EntryPoint = "EndDocPrinter", ExactSpelling = true, SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "StartPagePrinter", ExactSpelling = true, SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "EndPagePrinter", ExactSpelling = true, SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "WritePrinter", ExactSpelling = true, SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);
    #endregion
}
