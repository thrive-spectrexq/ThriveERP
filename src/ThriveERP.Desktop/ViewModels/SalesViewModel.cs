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
using ThriveERP.Application.Features.Customers;

namespace ThriveERP.Desktop.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private string _title = "Sales & POS Terminal";

    [ObservableProperty]
    private ObservableCollection<SalesOrderDto> _salesOrders = new();

    [ObservableProperty]
    private ViewModelBase? _currentOverlay;

    // --- POS Properties ---
    
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public ObservableCollection<string> Categories { get; } = new(new[] { "All", "Electronics", "Apparel", "Beverages", "Supplies", "Hardware", "Services" });

    [ObservableProperty]
    private string _selectedCategory = "All";

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
    private string _barcodeQuery = string.Empty;

    public ObservableCollection<CustomerDto> AvailableCustomers { get; } = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    [ObservableProperty]
    private string _customerNameInput = string.Empty;

    [ObservableProperty]
    private string _customerPhoneInput = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _posProducts = new();

    [ObservableProperty]
    private ObservableCollection<CartItemViewModel> _cartItems = new();

    private ObservableCollection<ProductDto> _allProducts = new();
    
    public ObservableCollection<SaleItemDto> SelectedOrderItems { get; } = new();

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _discountPercent;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private decimal _tax;

    [ObservableProperty]
    private decimal _grandTotal;

    public int CartItemCount => CartItems.Sum(c => c.Quantity);
    public bool IsCartEmpty => !CartItems.Any();

    [ObservableProperty] private decimal _selectedOrderSubtotal;
    [ObservableProperty] private decimal _selectedOrderDiscount;
    [ObservableProperty] private decimal _selectedOrderTax;
    [ObservableProperty] private decimal _selectedOrderGrandTotal;

    // --- Sales Orders History Filters ---
    [ObservableProperty]
    private string _salesOrderSearchQuery = string.Empty;

    public ObservableCollection<string> OrderStatusFilters { get; } = new(new[] { "All Statuses", "Submitted", "Invoiced", "Paid", "Voided", "Draft" });

    [ObservableProperty]
    private string _selectedOrderStatusFilter = "All Statuses";

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
        await LoadCustomersAsync();
        await LoadReturnsLogAsync();
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            var customers = await _mediator.Send(new GetAllCustomersQuery());
            AvailableCustomers.Clear();
            foreach (var c in customers) AvailableCustomers.Add(c);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading POS customers: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AddByBarcode()
    {
        if (string.IsNullOrWhiteSpace(BarcodeQuery)) return;

        var q = BarcodeQuery.Trim().ToLower();
        var match = _allProducts.FirstOrDefault(p => 
            (!string.IsNullOrEmpty(p.Barcode) && p.Barcode.ToLower() == q) || 
            p.Sku.ToLower() == q || 
            p.Name.ToLower().Contains(q));

        if (match != null)
        {
            AddToCart(match);
            BarcodeQuery = string.Empty;
        }
        else
        {
            MainWindowViewModel.Instance?.ShowToast($"No product found for barcode/SKU '{BarcodeQuery}'");
        }
    }

    [RelayCommand]
    private async Task LoadSalesOrdersAsync()
    {
        var result = await _mediator.Send(new GetAllSalesOrdersQuery());
        SalesOrders = new ObservableCollection<SalesOrderDto>(result);
        ApplySalesOrdersFilter();
    }

    private async Task LoadProductsAsync()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        _allProducts = new ObservableCollection<ProductDto>(result);
        ApplyProductsFilter();
    }

    [RelayCommand]
    private void SetSelectedCategory(string category)
    {
        SelectedCategory = category;
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyProductsFilter();
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyProductsFilter();
    }

    private void ApplyProductsFilter()
    {
        var filtered = _allProducts.AsEnumerable();

        if (SelectedCategory != "All" && !string.IsNullOrEmpty(SelectedCategory))
        {
            filtered = filtered.Where(p => p.CategoryName != null && p.CategoryName.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var lowerQuery = SearchQuery.ToLower();
            filtered = filtered.Where(p => 
                p.Name.ToLower().Contains(lowerQuery) || 
                (p.Sku != null && p.Sku.ToLower().Contains(lowerQuery)) ||
                (p.Barcode != null && p.Barcode.ToLower().Contains(lowerQuery))
            );
        }

        PosProducts = new ObservableCollection<ProductDto>(filtered);
    }

    partial void OnSalesOrderSearchQueryChanged(string value)
    {
        ApplySalesOrdersFilter();
    }

    partial void OnSelectedOrderStatusFilterChanged(string value)
    {
        ApplySalesOrdersFilter();
    }

    private void ApplySalesOrdersFilter()
    {
        var filtered = SalesOrders.AsEnumerable();

        if (SelectedOrderStatusFilter != "All Statuses")
        {
            filtered = filtered.Where(so => so.Status.ToString().Equals(SelectedOrderStatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SalesOrderSearchQuery))
        {
            var lowerQuery = SalesOrderSearchQuery.ToLower();
            filtered = filtered.Where(so => 
                (so.OrderNumber != null && so.OrderNumber.ToLower().Contains(lowerQuery)) ||
                (so.Status.ToString().ToLower().Contains(lowerQuery))
            );
        }

        FilteredSalesOrders = new ObservableCollection<SalesOrderDto>(filtered);
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
            var newItem = new CartItemViewModel(product, this);
            newItem.PropertyChanged += (s, e) => CalculateTotals();
            CartItems.Add(newItem);
        }
        CalculateTotals();
        MainWindowViewModel.Instance?.ShowToast($"Added '{product.Name}' to cart");
    }

    [RelayCommand]
    private void SearchAndAdd()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        
        var product = PosProducts.FirstOrDefault();
        if (product != null)
        {
            AddToCart(product);
            SearchQuery = string.Empty;
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
        MainWindowViewModel.Instance?.ShowToast("Cart cleared");
    }

    [RelayCommand]
    private void SetDiscount(string percentStr)
    {
        if (decimal.TryParse(percentStr, out var pct))
        {
            DiscountPercent = pct;
            CalculateTotals();
            MainWindowViewModel.Instance?.ShowToast($"Applied {pct}% discount");
        }
    }

    [ObservableProperty]
    private ObservableCollection<string> _availablePaymentMethods = new(new[] { "Cash", "Mobile Money", "Credit/Debit Card", "Store Credit" });

    [ObservableProperty]
    private string _selectedPaymentMethod = "Cash";

    [ObservableProperty]
    private decimal _amountTendered;

    [ObservableProperty]
    private decimal _changeAmount;

    [ObservableProperty]
    private bool _insufficientCash;

    partial void OnAmountTenderedChanged(decimal value)
    {
        CalculateChange();
    }

    [RelayCommand]
    private void SetCashPreset(string amountStr)
    {
        if (amountStr == "Exact")
        {
            AmountTendered = GrandTotal;
        }
        else if (decimal.TryParse(amountStr, out var val))
        {
            AmountTendered = val;
        }
        CalculateChange();
    }

    private void CalculateChange()
    {
        if (SelectedPaymentMethod == "Cash")
        {
            ChangeAmount = Math.Max(0, AmountTendered - GrandTotal);
            InsufficientCash = AmountTendered < GrandTotal;
        }
        else
        {
            ChangeAmount = 0;
            AmountTendered = GrandTotal;
            InsufficientCash = false;
        }
    }

    [RelayCommand]
    private void Checkout()
    {
        if (!CartItems.Any()) return;
        AmountTendered = GrandTotal;
        CalculateChange();

        if (SelectedCustomer != null)
        {
            CustomerNameInput = SelectedCustomer.Name;
            CustomerPhoneInput = SelectedCustomer.Phone ?? string.Empty;
        }
        else
        {
            CustomerNameInput = string.Empty;
            CustomerPhoneInput = string.Empty;
        }

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
                DiscountAmount / Math.Max(1, CartItems.Count) // Pro-rated discount
            )).ToList();

            Guid? targetCustomerId = SelectedCustomer?.Id;

            // Optional customer creation if entered on the fly at checkout
            if (!targetCustomerId.HasValue && !string.IsNullOrWhiteSpace(CustomerNameInput))
            {
                try
                {
                    var newCust = await _mediator.Send(new CreateCustomerCommand(
                        CustomerNameInput.Trim(),
                        string.IsNullOrWhiteSpace(CustomerPhoneInput) ? null : CustomerPhoneInput.Trim(),
                        null,
                        null,
                        1000m,
                        true
                    ));
                    targetCustomerId = newCust.Id;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Optional customer creation skipped: {ex.Message}");
                }
            }

            var command = new CreateSalesOrderCommand(targetCustomerId, Guid.Empty, items);
            var savedOrder = await _mediator.Send(command);

            // Fetch full order with populated item names
            var fullOrder = await _mediator.Send(new GetSalesOrderByIdQuery(savedOrder.Id)) ?? savedOrder;

            // Populate transient payment properties for receipt printing
            fullOrder = fullOrder with 
            { 
                AmountTendered = this.AmountTendered, 
                ChangeGiven = this.ChangeAmount, 
                PaymentMethodUsed = this.SelectedPaymentMethod 
            };

            ShowPaymentOverlay = false;
            ClearCart();
            await LoadSalesOrdersAsync();

            // Select the newly created order in history so cashier can view it in history
            SelectedSalesOrder = SalesOrders.FirstOrDefault(o => o.Id == fullOrder.Id);

            // Real notification
            MainWindowViewModel.Instance?.AddNotification(
                "New Order Checkout Completed",
                $"Order #{fullOrder.OrderNumber} completed for {fullOrder.GrandTotal:C}.",
                "🛒",
                "#10B981",
                typeof(SalesViewModel)
            );

            try 
            {
                // Direct POS Receipt Printer hardware printing
                var posPrinterService = App.Services!.GetRequiredService<ThriveERP.Application.Common.Interfaces.IPosPrinterService>();
                var pdfService = App.Services!.GetRequiredService<ThriveERP.Application.Common.Interfaces.IPdfExportService>();
                var businessName = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessName")) ?? "Thrive Inc.";
                var businessPhone = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessPhone"));
                var businessAddress = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessAddress"));
                var footerNote = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("ReceiptFooterNote"));

                // Send to physical POS printer if connected
                await posPrinterService.PrintReceiptAsync(fullOrder, "Default System Printer", "80mm", true, true, businessName);

                // Export PDF receipt & open in viewer so cashier can view and print on screen immediately
                var downloadsPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
                var receiptPath = System.IO.Path.Combine(downloadsPath, $"Receipt_{fullOrder.OrderNumber}.pdf");
                
                using (var stream = System.IO.File.Create(receiptPath))
                {
                    await pdfService.ExportReceiptAsync(stream, fullOrder, businessName, businessPhone, businessAddress, footerNote);
                    await stream.FlushAsync();
                }
                
                MainWindowViewModel.Instance?.ShowToast($"Payment successful! Order #{fullOrder.OrderNumber} completed.");

                // Open PDF viewer so receipt is instantly viewable on screen
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = receiptPath,
                        UseShellExecute = true
                    });
                }
                catch { /* Ignore if viewer cannot launch */ }
            }
            catch (Exception printEx)
            {
                Console.WriteLine($"Receipt generation failed: {printEx.Message}");
                MainWindowViewModel.Instance?.ShowToast($"Payment successful for Order #{fullOrder.OrderNumber}, but receipt generation failed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Checkout failed: {ex.Message}");
            MainWindowViewModel.Instance?.ShowToast($"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task PrintReceiptAsync(SalesOrderDto? order)
    {
        var targetOrder = order ?? SelectedSalesOrder;
        if (targetOrder == null) return;

        try
        {
            var fullOrder = await _mediator.Send(new GetSalesOrderByIdQuery(targetOrder.Id)) ?? targetOrder;
            var posPrinterService = App.Services!.GetRequiredService<ThriveERP.Application.Common.Interfaces.IPosPrinterService>();
            var pdfService = App.Services!.GetRequiredService<ThriveERP.Application.Common.Interfaces.IPdfExportService>();
            var businessName = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessName")) ?? "Thrive Inc.";
            var businessPhone = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessPhone"));
            var businessAddress = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessAddress"));
            var footerNote = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("ReceiptFooterNote"));

            await posPrinterService.PrintReceiptAsync(fullOrder, "Default System Printer", "80mm", true, true, businessName);

            var downloadsPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
            var receiptPath = System.IO.Path.Combine(downloadsPath, $"Receipt_{fullOrder.OrderNumber}.pdf");
            
            using (var stream = System.IO.File.Create(receiptPath))
            {
                await pdfService.ExportReceiptAsync(stream, fullOrder, businessName, businessPhone, businessAddress, footerNote);
                await stream.FlushAsync();
            }

            MainWindowViewModel.Instance?.ShowToast($"Printing receipt for Order #{fullOrder.OrderNumber}");

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = receiptPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PrintReceipt error: {ex.Message}");
        }
    }

    public void CalculateTotals()
    {
        Subtotal = CartItems.Sum(c => c.LineTotal);
        DiscountAmount = Subtotal * (DiscountPercent / 100m);
        var netSubtotal = Math.Max(0, Subtotal - DiscountAmount);
        Tax = netSubtotal * 0.15m; // 15% tax
        GrandTotal = netSubtotal + Tax;

        OnPropertyChanged(nameof(CartItemCount));
        OnPropertyChanged(nameof(IsCartEmpty));
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
            SelectedOrderSubtotal = details.Items.Sum(i => i.UnitPrice * i.Quantity);
            SelectedOrderDiscount = details.Items.Sum(i => i.DiscountAmount);
            SelectedOrderTax = (SelectedOrderSubtotal - SelectedOrderDiscount) * 0.10m;
            SelectedOrderGrandTotal = details.GrandTotal;
        }
        else
        {
            SelectedOrderSubtotal = 0;
            SelectedOrderDiscount = 0;
            SelectedOrderTax = 0;
            SelectedOrderGrandTotal = 0;
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
            MainWindowViewModel.Instance?.ShowToast("Sales Order created successfully");
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

    public ObservableCollection<ThriveERP.Application.Features.Returns.ReturnDto> ReturnsLog { get; } = new();

    [RelayCommand]
    private async Task LoadReturnsLogAsync()
    {
        try
        {
            var returns = await _mediator.Send(new ThriveERP.Application.Features.Returns.GetAllReturnsQuery());
            ReturnsLog.Clear();
            foreach (var r in returns) ReturnsLog.Add(r);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading returns log: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ExportCreditNoteAsync(ThriveERP.Application.Features.Returns.ReturnDto? returnItem)
    {
        if (returnItem == null) return;
        try
        {
            var pdfService = App.Services!.GetRequiredService<ThriveERP.Application.Common.Interfaces.IPdfExportService>();
            var businessName = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessName")) ?? "Thrive Inc.";
            var businessPhone = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessPhone"));
            var businessAddress = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessAddress"));
            var downloadsPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
            var creditNotePath = System.IO.Path.Combine(downloadsPath, $"CreditNote_{returnItem.OrderNumber}_{returnItem.Id.ToString().Substring(0, 6)}.pdf");

            using (var stream = System.IO.File.Create(creditNotePath))
            {
                await pdfService.ExportCreditNoteAsync(stream, returnItem, businessName, businessPhone, businessAddress);
                await stream.FlushAsync();
            }

            MainWindowViewModel.Instance?.ShowToast($"Credit note exported for Return #{returnItem.OrderNumber}");

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = creditNotePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ExportCreditNote error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ProcessReturnAsync()
    {
        if (SelectedSalesOrder == null || SelectedSaleItemForReturn == null) return;
        if (ReturnQuantity <= 0 || ReturnQuantity > SelectedSaleItemForReturn.Quantity) return;
        
        var cmd = new ThriveERP.Application.Features.Returns.ProcessReturnCommand(
            SelectedSalesOrder.Id, 
            SelectedSaleItemForReturn.ProductId, 
            ReturnQuantity, 
            ReturnReason);
            
        var result = await _mediator.Send(cmd);
        if (result)
        {
            ShowReturnOverlay = false;
            await LoadOrderDetailsAsync(SelectedSalesOrder.Id);
            await LoadReturnsLogAsync();

            MainWindowViewModel.Instance?.AddNotification(
                "Customer Return Processed",
                $"Returned {ReturnQuantity}x {SelectedSaleItemForReturn.ProductName} for Order #{SelectedSalesOrder.OrderNumber}.",
                "↩️",
                "#EF4444",
                typeof(SalesViewModel)
            );

            MainWindowViewModel.Instance?.ShowToast($"Return processed for {SelectedSaleItemForReturn.ProductName}");
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
            
            MainWindowViewModel.Instance?.ShowToast($"Invoice PDF exported for Order #{SelectedSalesOrder.OrderNumber}");

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
    private readonly SalesViewModel? _parentVm;

    public Guid ProductId { get; }
    public string Name { get; }
    public decimal UnitPrice { get; }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    private int _quantity;

    public decimal LineTotal => Quantity * UnitPrice;

    public CartItemViewModel(ProductDto product, SalesViewModel? parentVm = null)
    {
        _parentVm = parentVm;
        ProductId = product.Id;
        Name = product.Name;
        UnitPrice = product.SellingPrice;
        Quantity = 1;
    }

    [RelayCommand]
    private void IncrementQuantity()
    {
        Quantity++;
        _parentVm?.CalculateTotals();
    }

    [RelayCommand]
    private void DecrementQuantity()
    {
        if (Quantity > 1)
        {
            Quantity--;
            _parentVm?.CalculateTotals();
        }
    }
}

public class OrderStatusColorConverter : Avalonia.Data.Converters.IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        var status = value?.ToString();
        bool isBg = parameter?.ToString() == "bg";
        return status switch
        {
            "Submitted" => isBg ? "#DBEAFE" : "#1D4ED8", // blue
            "Paid" => isBg ? "#D1FAE5" : "#059669", // green
            "Voided" => isBg ? "#FEE2E2" : "#DC2626", // red
            "Draft" => isBg ? "#F3F4F6" : "#4B5563", // gray
            "Invoiced" => isBg ? "#FEF3C7" : "#D97706", // amber
            _ => isBg ? "#E5E7EB" : "#374151" // default
        };
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}
