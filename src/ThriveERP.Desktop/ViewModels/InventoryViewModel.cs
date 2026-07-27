using System;
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
    private string _title = "Inventory & Warehouses";

    [ObservableProperty]
    private ObservableCollection<StockLevelDto> _stockLevels = new();

    [ObservableProperty]
    private ObservableCollection<StockLevelDto> _filteredStockLevels = new();

    [ObservableProperty]
    private StockLevelDto? _selectedStockLevel;

    public ObservableCollection<string> WarehouseFilters { get; } = new(new[] { "All Warehouses", "Main Warehouse", "Secondary Warehouse" });

    [ObservableProperty]
    private string _selectedWarehouse = "All Warehouses";

    [ObservableProperty]
    private bool _onlyLowStock;

    // --- QUICK ADJUST STOCK MODAL ---
    [ObservableProperty]
    private bool _showAdjustModal;

    [ObservableProperty]
    private decimal _adjustmentQuantity = 10;

    [ObservableProperty]
    private string _adjustmentReason = "Stock Count Audit";

    public ObservableCollection<StockMovementLogItem> SelectedStockMovements { get; } = new();

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

    public InventoryViewModel() { } // designer

    partial void OnSelectedWarehouseChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnOnlyLowStockChanged(bool value)
    {
        ApplyFilter();
    }

    partial void OnSelectedStockLevelChanged(StockLevelDto? value)
    {
        if (value != null)
        {
            LoadStockMovements(value);
        }
        else
        {
            SelectedStockMovements.Clear();
        }
    }

    private void LoadStockMovements(StockLevelDto level)
    {
        SelectedStockMovements.Clear();
        SelectedStockMovements.Add(new StockMovementLogItem("Inbound Shipment PO #9001", "+50", DateTime.Now.AddDays(-2).ToString("g"), "Admin"));
        SelectedStockMovements.Add(new StockMovementLogItem("Sales Order #SO-0042", "-2", DateTime.Now.AddHours(-4).ToString("g"), "POS Cashier"));
        SelectedStockMovements.Add(new StockMovementLogItem("Manual Audit Adjustment", "+5", DateTime.Now.AddDays(-5).ToString("g"), "Store Manager"));
    }

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
        var filtered = StockLevels.AsEnumerable();

        if (SelectedWarehouse != "All Warehouses")
        {
            filtered = filtered.Where(s => s.WarehouseName.Equals(SelectedWarehouse, StringComparison.OrdinalIgnoreCase));
        }

        if (OnlyLowStock)
        {
            filtered = filtered.Where(s => s.QuantityOnHand <= 10);
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.ToLower();
            filtered = filtered.Where(s => s.ProductName.ToLower().Contains(q) || 
                                         s.WarehouseName.ToLower().Contains(q));
        }

        FilteredStockLevels = new ObservableCollection<StockLevelDto>(filtered);
    }

    [RelayCommand]
    private void OpenAdjustStock()
    {
        if (SelectedStockLevel == null) return;
        AdjustmentQuantity = 5;
        AdjustmentReason = "Inventory Audit";
        ShowAdjustModal = true;
    }

    [RelayCommand]
    private void CancelAdjustStock()
    {
        ShowAdjustModal = false;
    }

    [RelayCommand]
    private async Task ConfirmAdjustStockAsync()
    {
        if (SelectedStockLevel == null) return;

        ShowAdjustModal = false;
        MainWindowViewModel.Instance?.ShowToast($"Adjusted stock for '{SelectedStockLevel.ProductName}' by +{AdjustmentQuantity}");
        await LoadStockLevelsAsync();
    }
}

public record StockMovementLogItem(string Type, string Quantity, string Date, string User);
