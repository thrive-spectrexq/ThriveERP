using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Purchasing;
using ThriveERP.Application.Features.Suppliers;
using ThriveERP.Application.Features.Products;
using ThriveERP.Domain.Entities;
using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Desktop.ViewModels;

public partial class AddPurchaseOrderViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IWarehouseRepository _warehouseRepository;

    [ObservableProperty]
    private ObservableCollection<SupplierDto> _suppliers = new();

    [ObservableProperty]
    private SupplierDto? _selectedSupplier;

    [ObservableProperty]
    private ObservableCollection<Warehouse> _warehouses = new();

    [ObservableProperty]
    private Warehouse? _selectedWarehouse;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _availableProducts = new();

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private ObservableCollection<CreatePurchaseItemCommand> _items = new();

    [ObservableProperty]
    private decimal _quantityToAdd = 1;

    [ObservableProperty]
    private decimal _costToAdd;

    public Action? OnSaveComplete { get; set; }
    public Action? OnCancel { get; set; }

    public AddPurchaseOrderViewModel(IMediator mediator, IWarehouseRepository warehouseRepository)
    {
        _mediator = mediator;
        _warehouseRepository = warehouseRepository;
        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        var suppliers = await _mediator.Send(new GetAllSuppliersQuery());
        Suppliers = new ObservableCollection<SupplierDto>(suppliers);

        var products = await _mediator.Send(new GetAllProductsQuery());
        AvailableProducts = new ObservableCollection<ProductDto>(products);

        var whs = await _warehouseRepository.GetAllAsync();
        Warehouses = new ObservableCollection<Warehouse>(whs);
        if (whs.Any()) SelectedWarehouse = whs.First();
    }

    partial void OnSelectedProductChanged(ProductDto? value)
    {
        if (value != null)
        {
            CostToAdd = value.CostPrice;
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        if (SelectedProduct != null && QuantityToAdd > 0)
        {
            Items.Add(new CreatePurchaseItemCommand(
                SelectedProduct.Id, 
                QuantityToAdd, 
                CostToAdd
            ));
            SelectedProduct = null;
            QuantityToAdd = 1;
            CostToAdd = 0;
        }
    }

    [RelayCommand]
    private void RemoveItem(CreatePurchaseItemCommand item)
    {
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedSupplier == null || SelectedWarehouse == null || !Items.Any()) return;

        var command = new CreatePurchaseOrderCommand(
            SelectedSupplier.Id,
            SelectedWarehouse.Id,
            null,
            Items.ToList()
        );

        await _mediator.Send(command);
        OnSaveComplete?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCancel?.Invoke();
    }
}
