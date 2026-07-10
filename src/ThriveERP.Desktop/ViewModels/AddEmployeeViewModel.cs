using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System;
using System.Threading.Tasks;
using ThriveERP.Application.Features.HR;

namespace ThriveERP.Desktop.ViewModels;

public partial class AddEmployeeViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _position = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _hireDate = DateTimeOffset.Now;

    public Action? OnSave { get; set; }
    public Action? OnCancel { get; set; }

    public AddEmployeeViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void Reset()
    {
        FullName = string.Empty;
        Position = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        HireDate = DateTimeOffset.Now;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
            return;

        DateOnly? parsedHireDate = null;
        if (HireDate.HasValue)
        {
            parsedHireDate = DateOnly.FromDateTime(HireDate.Value.DateTime);
        }

        var command = new CreateEmployeeCommand(
            FullName,
            string.IsNullOrWhiteSpace(Position) ? null : Position,
            string.IsNullOrWhiteSpace(Phone) ? null : Phone,
            string.IsNullOrWhiteSpace(Email) ? null : Email,
            parsedHireDate
        );

        await _mediator.Send(command);
        OnSave?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCancel?.Invoke();
    }
}
