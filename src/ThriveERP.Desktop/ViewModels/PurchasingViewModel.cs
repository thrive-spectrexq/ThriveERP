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
    private readonly IMediator _mediator;

    [ObservableProperty]
    private string _title = "Purchasing / POs";

    [ObservableProperty]
    private ObservableCollection<PurchaseOrderDto> _purchaseOrders = new();

    [ObservableProperty]
    private ObservableCollection<PurchaseOrderDto> _filteredOrders = new();

    [ObservableProperty]
    private PurchaseOrderDto? _selectedOrder;

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
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredOrders = new ObservableCollection<PurchaseOrderDto>(PurchaseOrders);
        }
        else
        {
            var q = SearchQuery.ToLower();
            FilteredOrders = new ObservableCollection<PurchaseOrderDto>(
                PurchaseOrders.Where(o => o.OrderNumber.ToLower().Contains(q) || 
                                          o.SupplierName.ToLower().Contains(q) ||
                                          o.Status.ToLower().Contains(q)));
        }
    }

    [RelayCommand]
    private void ShowAddOrder()
    {
        var addVm = App.Services.GetRequiredService<AddPurchaseOrderViewModel>();
        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadOrdersCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }
}
