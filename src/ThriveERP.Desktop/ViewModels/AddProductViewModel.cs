using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Products;

namespace ThriveERP.Desktop.ViewModels;

public partial class AddProductViewModel : ObservableValidator
{
    private readonly IMediator _mediator;
    
    [ObservableProperty]
    [Required(ErrorMessage = "SKU is required")]
    [MinLength(3, ErrorMessage = "SKU must be at least 3 characters")]
    private string _sku = string.Empty;

    [ObservableProperty]
    private string? _barcode;

    [ObservableProperty]
    [Required(ErrorMessage = "Name is required")]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private decimal _costPrice;

    [ObservableProperty]
    private decimal _sellingPrice;

    [ObservableProperty]
    private bool _trackBatches;

    [ObservableProperty]
    private int _reorderThreshold;

    public Action? OnSaveComplete { get; set; }
    public Action? OnCancel { get; set; }

    public AddProductViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
            return;

        var command = new CreateProductCommand(
            Sku,
            Barcode,
            Name,
            Description,
            null, 
            null, 
            CostPrice,
            SellingPrice,
            TrackBatches,
            ReorderThreshold,
            true 
        );

        try
        {
            await _mediator.Send(command);
            OnSaveComplete?.Invoke();
        }
        catch (Exception)
        {
            // Handle error in real app (e.g., show dialog)
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCancel?.Invoke();
    }
}
