using System.Collections.ObjectModel;
using System.Linq;
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
    private string _title = "Suppliers / Vendors";

    [ObservableProperty]
    private ObservableCollection<SupplierDto> _suppliers = new();

    [ObservableProperty]
    private ObservableCollection<SupplierDto> _filteredSuppliers = new();

    [ObservableProperty]
    private SupplierDto? _selectedSupplier;

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
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredSuppliers = new ObservableCollection<SupplierDto>(Suppliers);
        }
        else
        {
            var q = SearchQuery.ToLower();
            FilteredSuppliers = new ObservableCollection<SupplierDto>(
                Suppliers.Where(s => s.Name.ToLower().Contains(q) || 
                                    (s.Email != null && s.Email.ToLower().Contains(q)) ||
                                    (s.Phone != null && s.Phone.ToLower().Contains(q))));
        }
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
    private void EditSupplier(SupplierDto? supplier)
    {
        var target = supplier ?? SelectedSupplier;
        if (target == null) return;
        var addVm = App.Services.GetRequiredService<AddSupplierViewModel>();
        addVm.Id = target.Id;
        addVm.Name = target.Name;
        addVm.Phone = target.Phone;
        addVm.Email = target.Email;
        addVm.Address = target.Address;

        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadSuppliersCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;

        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private async Task DeleteSupplierAsync(SupplierDto? supplier)
    {
        var target = supplier ?? SelectedSupplier;
        if (target == null || _mediator == null) return;
        await _mediator.Send(new DeleteSupplierCommand(target.Id));
        SelectedSupplier = null;
        await LoadSuppliersAsync();
    }
}
