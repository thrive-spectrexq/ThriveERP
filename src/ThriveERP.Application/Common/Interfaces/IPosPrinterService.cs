using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Features.Sales;

namespace ThriveERP.Application.Common.Interfaces;

public interface IPosPrinterService
{
    /// <summary>
    /// Gets a list of all installed printers connected to the system.
    /// </summary>
    List<string> GetInstalledPrinters();

    /// <summary>
    /// Prints a sales receipt directly to a connected POS receipt printer or system printer.
    /// </summary>
    Task<bool> PrintReceiptAsync(
        SalesOrderDto order,
        string printerName,
        string paperSize = "80mm",
        bool autoCut = true,
        bool openCashDrawer = true,
        string businessName = "ThriveERP",
        CancellationToken ct = default);

    /// <summary>
    /// Prints a test receipt to verify connection to the POS printer.
    /// </summary>
    Task<bool> PrintTestReceiptAsync(
        string printerName,
        string paperSize = "80mm",
        string businessName = "ThriveERP",
        CancellationToken ct = default);
}
