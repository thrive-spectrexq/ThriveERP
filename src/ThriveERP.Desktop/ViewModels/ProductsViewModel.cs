using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Features.Products;

namespace ThriveERP.Desktop.ViewModels;

public partial class ProductsViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private string _title = "Products Catalog";

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> _filteredProducts = new();

    [ObservableProperty]
    private ProductDto? _selectedProduct;

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

    public ProductsViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        Products.Clear();
        foreach (var product in result)
        {
            Products.Add(product);
        }
        ApplyFilter();
    }

    [RelayCommand]
    private async Task PrintLabelAsync(ProductDto? product)
    {
        var targetProduct = product ?? SelectedProduct;
        if (targetProduct == null) return;
        try
        {
            var labelService = App.Services!.GetRequiredService<ThriveERP.Application.Common.Interfaces.IBarcodeLabelService>();
            var desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            var outputDir = System.IO.Path.Combine(desktop, "ThriveERP_Labels");
            
            // Map the DTO back to a Domain Entity just for the barcode generation if needed, 
            // or pass the specific fields. Let's create a minimal Product entity.
            var productEntity = new ThriveERP.Domain.Entities.Product 
            { 
                Sku = targetProduct.Sku, 
                Name = targetProduct.Name, 
                SellingPrice = targetProduct.SellingPrice,
                Barcode = targetProduct.Sku // fallback if no barcode
            };

            var resultPath = await labelService.GenerateLabelPdfAsync(productEntity, outputDir);
            
            // Open the generated PDF
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = resultPath,
                UseShellExecute = true
            });
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Label Print Error: {ex.Message}");
        }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredProducts = new ObservableCollection<ProductDto>(Products);
        }
        else
        {
            var q = SearchQuery.ToLower();
            FilteredProducts = new ObservableCollection<ProductDto>(
                Products.Where(p => p.Name.ToLower().Contains(q) || 
                                    p.Sku.ToLower().Contains(q) ||
                                    (p.Barcode != null && p.Barcode.Contains(q))));
        }
    }

    [RelayCommand]
    private void ShowAddProduct()
    {
        var addVm = App.Services!.GetRequiredService<AddProductViewModel>();
        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadProductsCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private void EditProduct(ProductDto? product)
    {
        var target = product ?? SelectedProduct;
        if (target == null) return;

        var addVm = App.Services!.GetRequiredService<AddProductViewModel>();
        addVm.Id = target.Id;
        addVm.Sku = target.Sku;
        addVm.Name = target.Name;
        addVm.Barcode = target.Barcode;
        addVm.Description = target.Description;
        addVm.CostPrice = target.CostPrice;
        addVm.SellingPrice = target.SellingPrice;

        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadProductsCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private async Task DeleteProductAsync(ProductDto? product)
    {
        var target = product ?? SelectedProduct;
        if (target == null) return;
        
        await _mediator.Send(new DeleteProductCommand(target.Id));
        SelectedProduct = null;
        LoadProductsCommand.Execute(null);
    }
}
