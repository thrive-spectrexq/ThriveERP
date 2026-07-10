using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Features.Customers;

namespace ThriveERP.Desktop.ViewModels;

public partial class CustomersViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private string _title = "Customers Management";

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

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
    }

    [RelayCommand]
    private void ShowAddCustomer()
    {
        var addVm = App.Services.GetRequiredService<AddCustomerViewModel>();
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
        if (customer == null) return;
        var addVm = App.Services.GetRequiredService<AddCustomerViewModel>();
        addVm.Id = customer.Id;
        addVm.Name = customer.Name;
        addVm.Phone = customer.Phone;
        addVm.Email = customer.Email;
        addVm.Address = customer.Address;
        addVm.CreditLimit = customer.CreditLimit;

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
        if (customer == null) return;
        
        await _mediator.Send(new DeleteCustomerCommand(customer.Id));
        LoadCustomersCommand.Execute(null);
    }
}
