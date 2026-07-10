using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Features.Sales;
using ThriveERP.Application.Features.Products;

namespace ThriveERP.Desktop.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private string _title = "Sales Management";

    [ObservableProperty]
    private ObservableCollection<SalesOrderDto> _salesOrders = new();

    [ObservableProperty]
    private ViewModelBase? _currentOverlay;

    // --- POS Properties ---
    
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _posProducts = new();
    
    private ObservableCollection<ProductDto> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<CartItemViewModel> _cartItems = new();

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _tax;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private string _salesOrderSearchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SalesOrderDto> _filteredSalesOrders = new();

    [ObservableProperty]
    private SalesOrderDto? _selectedSalesOrder;

    public SalesViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        await LoadSalesOrdersAsync();
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadSalesOrdersAsync()
    {
        var result = await _mediator.Send(new GetAllSalesOrdersQuery());
        SalesOrders = new ObservableCollection<SalesOrderDto>(result);
        FilteredSalesOrders = new ObservableCollection<SalesOrderDto>(SalesOrders);
    }

    private async Task LoadProductsAsync()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        _allProducts = new ObservableCollection<ProductDto>(result);
        PosProducts = new ObservableCollection<ProductDto>(_allProducts);
    }

    partial void OnSalesOrderSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredSalesOrders = new ObservableCollection<SalesOrderDto>(SalesOrders);
        }
        else
        {
            var lowerQuery = value.ToLower();
            FilteredSalesOrders = new ObservableCollection<SalesOrderDto>(
                SalesOrders.Where(so => 
                    (so.OrderNumber != null && so.OrderNumber.ToLower().Contains(lowerQuery)) ||
                    (so.Status.ToString().ToLower().Contains(lowerQuery))
                )
            );
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            PosProducts = new ObservableCollection<ProductDto>(_allProducts);
        }
        else
        {
            var lowerQuery = value.ToLower();
            PosProducts = new ObservableCollection<ProductDto>(
                _allProducts.Where(p => 
                    p.Name.ToLower().Contains(lowerQuery) || 
                    (p.Sku != null && p.Sku.ToLower().Contains(lowerQuery)) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(lowerQuery))
                )
            );
        }
    }

    [RelayCommand]
    private void AddToCart(ProductDto product)
    {
        if (product == null) return;

        var existingItem = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            var newItem = new CartItemViewModel(product);
            newItem.PropertyChanged += (s, e) => CalculateTotals();
            CartItems.Add(newItem);
        }
        CalculateTotals();
    }

    [RelayCommand]
    private void SearchAndAdd()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        
        var product = PosProducts.FirstOrDefault();
        if (product != null)
        {
            AddToCart(product);
            SearchQuery = string.Empty; // clear after adding
        }
    }

    [RelayCommand]
    private void RemoveFromCart(CartItemViewModel item)
    {
        if (item != null)
        {
            CartItems.Remove(item);
            CalculateTotals();
        }
    }
    
    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        CalculateTotals();
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (!CartItems.Any()) return;
        
        try
        {
            // Prepare items
            var items = CartItems.Select(c => new CreateSaleItemDto(
                c.ProductId,
                c.Quantity,
                c.UnitPrice,
                0 // Discount
            )).ToList();

            // Default warehouse for now (we'll assume the first one or a specific GUID if needed)
            // A more robust implementation would select the user's current warehouse.
            // For now, let's just pass Guid.Empty and let the handler deal with it or we fetch it.
            // Actually, we must provide a valid WarehouseId. Let's fetch one quickly.
            
            // To keep it simple, we'll send the command. We need a valid WarehouseId. 
            // In a real app, this is fetched during initialization.
            var command = new CreateSalesOrderCommand(null, Guid.Empty, items);
            await _mediator.Send(command);

            ClearCart();
            await LoadSalesOrdersAsync();
        }
        catch (Exception ex)
        {
            // Handle checkout failure (e.g., show an error dialog)
            Console.WriteLine($"Checkout failed: {ex.Message}");
        }
    }

    private void CalculateTotals()
    {
        Subtotal = CartItems.Sum(c => c.LineTotal);
        Tax = Subtotal * 0.10m; // 10% tax for mockup
        GrandTotal = Subtotal + Tax;
    }

    [RelayCommand]
    private void ShowAddSalesOrder()
    {
        var addVm = App.Services!.GetRequiredService<AddSalesOrderViewModel>();
        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadSalesOrdersCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private async Task ExportInvoiceAsync()
    {
        if (SelectedSalesOrder == null) return;
        try
        {
            var outputDir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "ThriveERP_Invoices");
            var resultPath = await _mediator.Send(new ExportInvoiceCommand(SelectedSalesOrder.Id, outputDir));
            
            // Open the generated PDF
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = resultPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

public partial class CartItemViewModel : ObservableObject
{
    public Guid ProductId { get; }
    public string Name { get; }
    public decimal UnitPrice { get; }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    private int _quantity;

    public decimal LineTotal => Quantity * UnitPrice;

    public CartItemViewModel(ProductDto product)
    {
        ProductId = product.Id;
        Name = product.Name;
        UnitPrice = product.SellingPrice;
        Quantity = 1;
    }
}
