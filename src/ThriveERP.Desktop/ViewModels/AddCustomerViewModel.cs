using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Customers;

namespace ThriveERP.Desktop.ViewModels;

public partial class AddCustomerViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private Guid? _id;

    [ObservableProperty]
    [Required(ErrorMessage = "Name is required")]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _phone;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private decimal _creditLimit;

    public Action? OnSaveComplete { get; set; }
    public Action? OnCancel { get; set; }

    public AddCustomerViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
            return;

        try
        {
            if (Id.HasValue && Id.Value != Guid.Empty)
            {
                var command = new UpdateCustomerCommand(
                    Id.Value,
                    Name,
                    Phone,
                    Email,
                    Address,
                    CreditLimit,
                    true
                );
                await _mediator.Send(command);
            }
            else
            {
                var command = new CreateCustomerCommand(
                    Name,
                    Phone,
                    Email,
                    Address,
                    CreditLimit,
                    true
                );
                await _mediator.Send(command);
            }

            OnSaveComplete?.Invoke();
        }
        catch (Exception)
        {
            // TODO: Error handling
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCancel?.Invoke();
    }
}
