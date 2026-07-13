using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Products;
using ThriveERP.Application.Features.Sales;
using ThriveERP.Application.Features.Customers;

namespace ThriveERP.Desktop.ViewModels;

public partial class SaleItemViewModel : ObservableObject
{
    private ProductDto? _selectedProduct;
    public ProductDto? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value))
            {
                if (value != null)
                {
                    UnitPrice = value.SellingPrice;
                }
                UpdateLineTotal();
            }
        }
    }

    [ObservableProperty]
    private decimal _quantity = 1;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private decimal _lineTotal;

    partial void OnQuantityChanged(decimal value) => UpdateLineTotal();
    partial void OnUnitPriceChanged(decimal value) => UpdateLineTotal();
    partial void OnDiscountAmountChanged(decimal value) => UpdateLineTotal();

    private void UpdateLineTotal()
    {
        LineTotal = (Quantity * UnitPrice) - DiscountAmount;
    }
}

public partial class AddSalesOrderViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _availableProducts = new();

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _availableCustomers = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    public ObservableCollection<SaleItemViewModel> Items { get; } = new();

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
        LoadDataCommand.Execute(null);
        Items.CollectionChanged += (s, e) => CalculateTotals();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var products = await _mediator.Send(new GetAllProductsQuery());
        AvailableProducts = new ObservableCollection<ProductDto>(products);

        var customers = await _mediator.Send(new ThriveERP.Application.Features.Customers.GetAllCustomersQuery());
        AvailableCustomers = new ObservableCollection<CustomerDto>(customers);
    }

    [RelayCommand]
    private void AddItem()
    {
        var item = new SaleItemViewModel();
        item.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(SaleItemViewModel.LineTotal))
                CalculateTotals();
        };
        Items.Add(item);
    }

    [RelayCommand]
    private void RemoveItem(SaleItemViewModel item)
    {
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    private void CalculateTotals()
    {
        Subtotal = Items.Sum(i => i.LineTotal);
        TaxTotal = 0; // Tax logic
        GrandTotal = Subtotal + TaxTotal;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var warehouseId = Guid.NewGuid(); // Dummy warehouse ID for MVP
        
        var dtoList = Items.Where(i => i.SelectedProduct != null)
                           .Select(i => new CreateSaleItemDto(i.SelectedProduct!.Id, i.Quantity, i.UnitPrice, i.DiscountAmount))
                           .ToList();

        if (!dtoList.Any()) return; // Must have at least 1 valid item

        var command = new CreateSalesOrderCommand(
            SelectedCustomer?.Id,
            warehouseId,
            dtoList
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
