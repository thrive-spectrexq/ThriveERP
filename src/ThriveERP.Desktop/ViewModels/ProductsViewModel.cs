using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Features.Products;

namespace ThriveERP.Desktop.ViewModels;

public partial class ProductsViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private string _title = "Products Management";

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

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
        Products = new ObservableCollection<ProductDto>(result);
    }

    [RelayCommand]
    private void ShowAddProduct()
    {
        var addVm = App.Services.GetRequiredService<AddProductViewModel>();
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
        if (product == null) return;
        var addVm = App.Services.GetRequiredService<AddProductViewModel>();
        addVm.Id = product.Id;
        addVm.Sku = product.Sku;
        addVm.Name = product.Name;
        addVm.Barcode = product.Barcode;
        addVm.Description = product.Description;
        addVm.CostPrice = product.CostPrice;
        addVm.SellingPrice = product.SellingPrice;

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
        if (product == null) return;
        
        await _mediator.Send(new DeleteProductCommand(product.Id));
        LoadProductsCommand.Execute(null);
    }
}
