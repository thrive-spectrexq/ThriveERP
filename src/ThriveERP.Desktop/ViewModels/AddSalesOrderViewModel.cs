using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Sales;

namespace ThriveERP.Desktop.ViewModels;

public partial class AddSalesOrderViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _taxTotal;

    [ObservableProperty]
    private decimal _grandTotal;

    public Action? OnSaveComplete { get; set; }
    public Action? OnCancel { get; set; }

    public AddSalesOrderViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var warehouseId = Guid.NewGuid(); // Dummy warehouse ID for MVP
        
        var command = new CreateSalesOrderCommand(
            null,
            warehouseId,
            Subtotal,
            TaxTotal,
            GrandTotal
        );

        try
        {
            await _mediator.Send(command);
            OnSaveComplete?.Invoke();
        }
        catch (Exception)
        {
            // Handle error in real app
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCancel?.Invoke();
    }
}
