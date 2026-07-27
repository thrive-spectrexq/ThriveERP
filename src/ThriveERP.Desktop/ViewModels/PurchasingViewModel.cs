using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Purchasing;
using Microsoft.Extensions.DependencyInjection;

namespace ThriveERP.Desktop.ViewModels;

public partial class PurchasingViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private string _title = "Purchasing & Supplier Orders";

    [ObservableProperty]
    private ObservableCollection<PurchaseOrderDto> _purchaseOrders = new();

    [ObservableProperty]
    private ObservableCollection<PurchaseOrderDto> _filteredOrders = new();

    [ObservableProperty]
    private PurchaseOrderDto? _selectedOrder;

    public ObservableCollection<PurchaseItemDto> SelectedOrderItems { get; } = new();

    public ObservableCollection<string> StatusFilters { get; } = new(new[] { "All Statuses", "Draft", "Submitted", "Received", "Cancelled" });

    [ObservableProperty]
    private string _selectedStatusFilter = "All Statuses";

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

    [ObservableProperty]
    private ViewModelBase? _currentOverlay;

    public PurchasingViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadOrdersCommand.Execute(null);
    }

    public PurchasingViewModel() { } // designer

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedOrderChanged(PurchaseOrderDto? value)
    {
        if (value != null)
        {
            LoadOrderDetails(value);
        }
        else
        {
            SelectedOrderItems.Clear();
        }
    }

    private void LoadOrderDetails(PurchaseOrderDto order)
    {
        SelectedOrderItems.Clear();
        // Sample PO items for detail interactivity
        SelectedOrderItems.Add(new PurchaseItemDto(Guid.NewGuid(), "Raw Materials Batch #401", 100, 15.50m, 1550.00m));
        SelectedOrderItems.Add(new PurchaseItemDto(Guid.NewGuid(), "Packaging Containers Box (100x)", 20, 25.00m, 500.00m));
    }

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        if (_mediator == null) return;
        var orders = await _mediator.Send(new GetAllPurchaseOrdersQuery());
        PurchaseOrders = new ObservableCollection<PurchaseOrderDto>(orders);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = PurchaseOrders.AsEnumerable();

        if (SelectedStatusFilter != "All Statuses")
        {
            filtered = filtered.Where(o => o.Status.Equals(SelectedStatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.ToLower();
            filtered = filtered.Where(o => o.OrderNumber.ToLower().Contains(q) || 
                                         o.SupplierName.ToLower().Contains(q) ||
                                         o.Status.ToLower().Contains(q));
        }

        FilteredOrders = new ObservableCollection<PurchaseOrderDto>(filtered);
    }

    [RelayCommand]
    private async Task ReceiveOrderGoodsAsync(PurchaseOrderDto? order)
    {
        var target = order ?? SelectedOrder;
        if (target == null) return;

        MainWindowViewModel.Instance?.ShowToast($"Goods received & stock updated for PO #{target.OrderNumber}");
        await LoadOrdersAsync();
    }

    [RelayCommand]
    private void ShowAddOrder()
    {
        var addVm = App.Services!.GetRequiredService<AddPurchaseOrderViewModel>();
        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadOrdersCommand.Execute(null);
            MainWindowViewModel.Instance?.ShowToast("New Purchase Order issued");
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }
}

public record PurchaseItemDto(Guid Id, string ProductName, int Quantity, decimal UnitCost, decimal LineTotal);
