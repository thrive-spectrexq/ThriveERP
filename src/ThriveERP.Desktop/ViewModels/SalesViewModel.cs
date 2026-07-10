using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Features.Sales;

namespace ThriveERP.Desktop.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private string _title = "Sales Management";

    [ObservableProperty]
    private ObservableCollection<SalesOrderDto> _salesOrders = new();

    [ObservableProperty]
    private ViewModelBase? _currentOverlay;

    public SalesViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadSalesOrdersCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadSalesOrdersAsync()
    {
        var result = await _mediator.Send(new GetAllSalesOrdersQuery());
        SalesOrders = new ObservableCollection<SalesOrderDto>(result);
    }

    [RelayCommand]
    private void ShowAddSalesOrder()
    {
        var addVm = App.Services.GetRequiredService<AddSalesOrderViewModel>();
        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadSalesOrdersCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }
}
