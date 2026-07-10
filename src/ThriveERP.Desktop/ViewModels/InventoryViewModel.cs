using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Inventory;

namespace ThriveERP.Desktop.ViewModels;

public partial class InventoryViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private string _title = "Inventory Management";

    [ObservableProperty]
    private ObservableCollection<StockLevelDto> _stockLevels = new();

    [ObservableProperty]
    private ObservableCollection<StockLevelDto> _filteredStockLevels = new();

    [ObservableProperty]
    private StockLevelDto? _selectedStockLevel;

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                ApplyFilter();
            }
        }
    }

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
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredStockLevels = new ObservableCollection<StockLevelDto>(StockLevels);
        }
        else
        {
            var q = SearchQuery.ToLower();
            FilteredStockLevels = new ObservableCollection<StockLevelDto>(
                StockLevels.Where(s => s.ProductName.ToLower().Contains(q) || 
                                       s.WarehouseName.ToLower().Contains(q)));
        }
    }
}
