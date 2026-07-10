using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Inventory;

namespace ThriveERP.Desktop.ViewModels;

public partial class InventoryViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<StockLevelDto> _stockLevels = new();

    public InventoryViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadStockLevelsCommand.Execute(null);
    }

    // Default constructor for designer
    public InventoryViewModel() { }

    [RelayCommand]
    private async Task LoadStockLevelsAsync()
    {
        if (_mediator == null) return;
        var levels = await _mediator.Send(new GetStockLevelsQuery(null, null));
        StockLevels = new ObservableCollection<StockLevelDto>(levels);
    }
}
