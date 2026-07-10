using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Features.Customers;

namespace ThriveERP.Desktop.ViewModels;

public partial class CustomersViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private string _title = "Customers Profile";

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _filteredCustomers = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

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

    public CustomersViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadCustomersCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        var result = await _mediator.Send(new GetAllCustomersQuery());
        Customers = new ObservableCollection<CustomerDto>(result);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredCustomers = new ObservableCollection<CustomerDto>(Customers);
        }
        else
        {
            var q = SearchQuery.ToLower();
            FilteredCustomers = new ObservableCollection<CustomerDto>(
                Customers.Where(c => c.Name.ToLower().Contains(q) || 
                                     (c.Email != null && c.Email.ToLower().Contains(q)) ||
                                     (c.Phone != null && c.Phone.Contains(q))));
        }
    }

    [RelayCommand]
    private void ShowAddCustomer()
    {
        var addVm = App.Services!.GetRequiredService<AddCustomerViewModel>();
        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadCustomersCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private void EditCustomer(CustomerDto? customer)
    {
        var target = customer ?? SelectedCustomer;
        if (target == null) return;

        var addVm = App.Services!.GetRequiredService<AddCustomerViewModel>();
        addVm.Id = target.Id;
        addVm.Name = target.Name;
        addVm.Phone = target.Phone;
        addVm.Email = target.Email;
        addVm.Address = target.Address;
        addVm.CreditLimit = target.CreditLimit;

        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadCustomersCommand.Execute(null);
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync(CustomerDto? customer)
    {
        var target = customer ?? SelectedCustomer;
        if (target == null) return;
        
        await _mediator.Send(new DeleteCustomerCommand(target.Id));
        SelectedCustomer = null;
        LoadCustomersCommand.Execute(null);
    }
}
