using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Infrastructure.Services;

public class ExcelExportService : IExcelExportService
{
    public Task ExportAsync<T>(Stream stream, T data, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");
        
        // This is a generic implementation. In a real scenario, you would inspect
        // the type T or use an interface to format the Excel sheet properly.
        // As a demonstration, if T is an IEnumerable<object>, we can output a grid.
        
        if (data is IEnumerable<object> list)
        {
            worksheet.Cell(1, 1).InsertTable(list);
            worksheet.Columns().AdjustToContents();
        }
        else
        {
            worksheet.Cell(1, 1).Value = "Report Data Generated";
            worksheet.Cell(2, 1).Value = data?.ToString();
        }

        workbook.SaveAs(stream);
        return Task.CompletedTask;
    }
}
