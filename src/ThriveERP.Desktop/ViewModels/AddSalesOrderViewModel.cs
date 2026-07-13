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

    private readonly ThriveERP.Application.Common.Interfaces.IPdfExportService _pdfService;

    public AddSalesOrderViewModel(IMediator mediator, ThriveERP.Application.Common.Interfaces.IPdfExportService pdfService)
    {
        _mediator = mediator;
        _pdfService = pdfService;
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

    [ObservableProperty]
    private bool _showPaymentOverlay;

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
    private void PayNow()
    {
        Console.WriteLine($"PayNow clicked. Items count: {Items.Count}. Valid items: {Items.Count(i => i.SelectedProduct != null)}");
        
        // Show overlay even if no items for debugging, but set total to 0
        if (!Items.Any(i => i.SelectedProduct != null)) 
        {
            Console.WriteLine("No items selected. Proceeding anyway for debug.");
        }

        AmountTendered = GrandTotal;
        CalculateChange();
        ShowPaymentOverlay = true;
        Console.WriteLine($"ShowPaymentOverlay is now {ShowPaymentOverlay}");
    }

    [RelayCommand]
    private void CancelPayment()
    {
        ShowPaymentOverlay = false;
    }

    [RelayCommand]
    private async Task ConfirmPaymentAsync()
    {
        var warehouseId = Guid.NewGuid(); // Dummy warehouse ID for MVP
        
        var dtoList = Items.Where(i => i.SelectedProduct != null)
                           .Select(i => new CreateSaleItemDto(i.SelectedProduct!.Id, i.Quantity, i.UnitPrice, i.DiscountAmount))
                           .ToList();

        if (!dtoList.Any()) return; 

        var command = new CreateSalesOrderCommand(
            SelectedCustomer?.Id,
            warehouseId,
            dtoList
        );

        try
        {
            var savedOrder = await _mediator.Send(command);
            
            var businessName = await _mediator.Send(new ThriveERP.Application.Features.Settings.GetSettingQuery("BusinessName")) ?? "Thrive Inc.";
            var downloadsPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
            var receiptPath = System.IO.Path.Combine(downloadsPath, $"Receipt_{savedOrder.OrderNumber}.pdf");
            
            using var stream = System.IO.File.Create(receiptPath);
            await _pdfService.ExportReceiptAsync(stream, savedOrder, businessName);

            // Close overlay and open the PDF automatically using standard process
            ShowPaymentOverlay = false;
            OnSaveComplete?.Invoke();
            
            try 
            {
                // On Windows, this will open the default PDF viewer which allows the user to print
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = receiptPath,
                    UseShellExecute = true
                });
            }
            catch { /* Ignore if it fails to open */ }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCancel?.Invoke();
    }
}
