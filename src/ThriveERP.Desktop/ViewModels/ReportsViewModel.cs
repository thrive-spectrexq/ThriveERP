using System;
using System.Collections.ObjectModel;
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
    private string _title = "Analytics & Reports Generator";

    public ObservableCollection<string> ReportTypes { get; } = new(new[]
    {
        "Daily Sales Summary",
        "Inventory Stock Audit",
        "P&L Financial Statement",
        "Cashier Performance Report"
    });

    [ObservableProperty]
    private string _selectedReportType = "Daily Sales Summary";

    [ObservableProperty]
    private DateTimeOffset _selectedDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string _previewTotalRevenue = "$4,250.00";

    [ObservableProperty]
    private string _previewOrderCount = "28 Orders";

    [ObservableProperty]
    private string _previewAverageOrder = "$151.78";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ReportsViewModel(IMediator mediator, IPdfExportService pdfService, IExcelExportService excelService)
    {
        _mediator = mediator;
        _pdfService = pdfService;
        _excelService = excelService;
        _ = LoadReportPreviewAsync();
    }

    partial void OnSelectedDateChanged(DateTimeOffset value)
    {
        _ = LoadReportPreviewAsync();
    }

    partial void OnSelectedReportTypeChanged(string value)
    {
        _ = LoadReportPreviewAsync();
    }

    private async Task LoadReportPreviewAsync()
    {
        try
        {
            var data = await _mediator.Send(new GetDailySalesReportQuery(SelectedDate.DateTime));
            PreviewTotalRevenue = data.TotalSales.ToString("C");
            PreviewOrderCount = $"{data.OrderCount} Orders";
            decimal avg = data.OrderCount > 0 ? data.TotalSales / data.OrderCount : 0m;
            PreviewAverageOrder = avg.ToString("C");
        }
        catch
        {
            // fallback designer
        }
    }

    [RelayCommand]
    private async Task GeneratePdfAsync()
    {
        try
        {
            StatusMessage = "Generating PDF Report...";
            var reportData = await _mediator.Send(new GetDailySalesReportQuery(SelectedDate.DateTime));
            
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var fileName = $"Report_{SelectedReportType.Replace(" ", "_")}_{SelectedDate:yyyyMMdd}.pdf";
            var filePath = Path.Combine(downloadsPath, fileName);

            using (var stream = File.Create(filePath))
            {
                await _pdfService.ExportAsync(stream, reportData);
                await stream.FlushAsync();
            }
            
            StatusMessage = $"Saved: {fileName}";
            MainWindowViewModel.Instance?.ShowToast($"PDF Report saved to Downloads: {fileName}");

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
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
            StatusMessage = "Generating Excel Report...";
            var reportData = await _mediator.Send(new GetDailySalesReportQuery(SelectedDate.DateTime));
            
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var fileName = $"Report_{SelectedReportType.Replace(" ", "_")}_{SelectedDate:yyyyMMdd}.xlsx";
            var filePath = Path.Combine(downloadsPath, fileName);

            var list = new[]
            {
                new { Report = SelectedReportType, Date = reportData.Date.ToString("yyyy-MM-dd"), Orders = reportData.OrderCount, TotalRevenue = reportData.TotalSales }
            };

            using (var stream = File.Create(filePath))
            {
                await _excelService.ExportAsync(stream, list);
                await stream.FlushAsync();
            }
            
            StatusMessage = $"Saved: {fileName}";
            MainWindowViewModel.Instance?.ShowToast($"Excel Report saved to Downloads: {fileName}");

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
