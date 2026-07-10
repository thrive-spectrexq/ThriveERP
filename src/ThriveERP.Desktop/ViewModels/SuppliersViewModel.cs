using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Suppliers;
using Microsoft.Extensions.DependencyInjection;

namespace ThriveERP.Desktop.ViewModels;

public partial class SuppliersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<SupplierDto> _suppliers = new();

    [ObservableProperty]
    private ViewModelBase? _currentOverlay;

    public SuppliersViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadSuppliersCommand.Execute(null);
    }

    // Default constructor for designer
    public SuppliersViewModel() { }

    [RelayCommand]
    private async Task LoadSuppliersAsync()
    {
        if (_mediator == null) return;
        var suppliersList = await _mediator.Send(new GetAllSuppliersQuery());
        Suppliers = new ObservableCollection<SupplierDto>(suppliersList);
    }

    [RelayCommand]
    private void ShowAddSupplier()
    {
        var addVm = App.Services.GetRequiredService<AddSupplierViewModel>();
        addVm.Id = null;
        addVm.Name = "";
        addVm.Phone = "";
        addVm.Email = "";
        addVm.Address = "";

        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadSuppliersCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private void EditSupplier(SupplierDto supplier)
    {
        if (supplier == null) return;
        var addVm = App.Services.GetRequiredService<AddSupplierViewModel>();
        addVm.Id = supplier.Id;
        addVm.Name = supplier.Name;
        addVm.Phone = supplier.Phone;
        addVm.Email = supplier.Email;
        addVm.Address = supplier.Address;

        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadSuppliersCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;

        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private async Task DeleteSupplierAsync(SupplierDto supplier)
    {
        if (supplier == null || _mediator == null) return;
        await _mediator.Send(new DeleteSupplierCommand(supplier.Id));
        await LoadSuppliersAsync();
    }
}
