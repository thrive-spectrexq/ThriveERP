using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Application.Features.Reports;

namespace ThriveERP.Desktop.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IPdfExportService _pdfService;
    private readonly IExcelExportService _excelService;

    [ObservableProperty]
    private DateTimeOffset _selectedDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ReportsViewModel(IMediator mediator, IPdfExportService pdfService, IExcelExportService excelService)
    {
        _mediator = mediator;
        _pdfService = pdfService;
        _excelService = excelService;
    }

    [RelayCommand]
    private async Task GeneratePdfAsync()
    {
        try
        {
            StatusMessage = "Generating PDF...";
            var reportData = await _mediator.Send(new GetDailySalesReportQuery(SelectedDate.DateTime));
            
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var fileName = $"DailySales_{SelectedDate:yyyyMMdd}.pdf";
            var filePath = Path.Combine(downloadsPath, fileName);

            using var stream = File.Create(filePath);
            await _pdfService.ExportAsync(stream, reportData);
            
            StatusMessage = $"Saved to {filePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task GenerateExcelAsync()
    {
        try
        {
            StatusMessage = "Generating Excel...";
            var reportData = await _mediator.Send(new GetDailySalesReportQuery(SelectedDate.DateTime));
            
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var fileName = $"DailySales_{SelectedDate:yyyyMMdd}.xlsx";
            var filePath = Path.Combine(downloadsPath, fileName);

            using var stream = File.Create(filePath);
            
            // Convert dto to simple list for generic excel export
            var list = new[]
            {
                new { Date = reportData.Date.ToString("yyyy-MM-dd"), Orders = reportData.OrderCount, Revenue = reportData.TotalSales }
            };
            
            await _excelService.ExportAsync(stream, list);
            
            StatusMessage = $"Saved to {filePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
