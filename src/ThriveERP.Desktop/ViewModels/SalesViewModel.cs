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
    private bool _showPaymentOverlay;
    
    [ObservableProperty]
    private bool _showReturnOverlay;

    [ObservableProperty]
    private SaleItemDto? _selectedSaleItemForReturn;

    [ObservableProperty]
    private decimal _returnQuantity;

    [ObservableProperty]
    private string _returnReason = string.Empty;
    [ObservableProperty]
    private ObservableCollection<ProductDto> _posProducts = new();

    [ObservableProperty]
    private ObservableCollection<CartItemViewModel> _cartItems = new();

    private ObservableCollection<ProductDto> _allProducts = new();
    
    public ObservableCollection<SaleItemDto> SelectedOrderItems { get; } = new();

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

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<string> _availablePaymentMethods = new(new[] { "Cash", "Mobile Money", "Credit/Debit Card", "Store Credit" });

    [ObservableProperty]
    private string _selectedPaymentMethod = "Cash";

    [ObservableProperty]
    private decimal _amountTendered;

    [ObservableProperty]
    private decimal _changeAmount;

    partial void OnAmountTenderedChanged(decimal value)
    {
        CalculateChange();
    }

    private void CalculateChange()
    {
        if (SelectedPaymentMethod == "Cash")
        {
            ChangeAmount = Math.Max(0, AmountTendered - GrandTotal);
        }
        else
        {
            ChangeAmount = 0;
            AmountTendered = GrandTotal;
        }
    }

    [RelayCommand]
    private void Checkout()
    {
        if (!CartItems.Any()) return;
        AmountTendered = GrandTotal;
        CalculateChange();
        ShowPaymentOverlay = true;
    }

    [RelayCommand]
    private void CancelPayment()
    {
        ShowPaymentOverlay = false;
    }

    [RelayCommand]
    private async Task ConfirmPaymentAsync()
    {
        if (!CartItems.Any()) return;
        
        try
        {
            var items = CartItems.Select(c => new CreateSaleItemDto(
                c.ProductId,
                c.Quantity,
                c.UnitPrice,
                0 // Discount
            )).ToList();

            var command = new CreateSalesOrderCommand(null, Guid.Empty, items);
            var savedOrder = await _mediator.Send(command);

            // Receipt logic
            var pdfService = App.Services!.GetRequiredService<ThriveERP.Application.Common.Interfaces.IPdfExportService>();
            var businessName = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessName")) ?? "Thrive Inc.";
            var downloadsPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
            var receiptPath = System.IO.Path.Combine(downloadsPath, $"Receipt_{savedOrder.OrderNumber}.pdf");
            
            using var stream = System.IO.File.Create(receiptPath);
            await pdfService.ExportReceiptAsync(stream, savedOrder, businessName);

            ShowPaymentOverlay = false;
            ClearCart();
            await LoadSalesOrdersAsync();

            try 
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = receiptPath,
                    UseShellExecute = true
                });
            }
            catch { /* Ignore */ }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Checkout failed: {ex.Message}");
        }
    }

    private void CalculateTotals()
    {
        Subtotal = CartItems.Sum(c => c.LineTotal);
        Tax = Subtotal * 0.10m; // 10% tax for mockup
        GrandTotal = Subtotal + Tax;
    }

    partial void OnSelectedSalesOrderChanged(SalesOrderDto? value)
    {
        if (value != null)
        {
            _ = LoadOrderDetailsAsync(value.Id);
        }
        else
        {
            SelectedOrderItems.Clear();
        }
    }

    private async Task LoadOrderDetailsAsync(Guid orderId)
    {
        var details = await _mediator.Send(new GetSalesOrderByIdQuery(orderId));
        SelectedOrderItems.Clear();
        if (details != null)
        {
            foreach(var item in details.Items) SelectedOrderItems.Add(item);
        }
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
    private void InitiateReturn(SaleItemDto item)
    {
        SelectedSaleItemForReturn = item;
        ReturnQuantity = 1;
        ReturnReason = string.Empty;
        ShowReturnOverlay = true;
    }

    [RelayCommand]
    private void CancelReturn()
    {
        ShowReturnOverlay = false;
        SelectedSaleItemForReturn = null;
    }

    [RelayCommand]
    private async Task ProcessReturnAsync()
    {
        if (SelectedSalesOrder == null || SelectedSaleItemForReturn == null) return;
        
        if (ReturnQuantity <= 0 || ReturnQuantity > SelectedSaleItemForReturn.Quantity) return; // Validation
        
        var cmd = new ThriveERP.Application.Features.Returns.ProcessReturnCommand(
            SelectedSalesOrder.Id, 
            SelectedSaleItemForReturn.ProductId, 
            ReturnQuantity, 
            ReturnReason);
            
        var result = await _mediator.Send(cmd);
        if (result)
        {
            ShowReturnOverlay = false;
            await LoadOrderDetailsAsync(SelectedSalesOrder.Id); // Refresh items
        }
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
