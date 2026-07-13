using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ThriveERP.Application.Features.HR;

namespace ThriveERP.Desktop.ViewModels;

public partial class AddEmployeeViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

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

    [ObservableProperty]
    private bool _createUserAccount;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RoleDto> _availableRoles = new();

    [ObservableProperty]
    private RoleDto? _selectedRole;

    public Action? OnSave { get; set; }
    public Action? OnCancel { get; set; }

    public AddEmployeeViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadRolesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadRolesAsync()
    {
        var roles = await _mediator.Send(new GetAllRolesQuery());
        AvailableRoles = new ObservableCollection<RoleDto>(roles);
    }

    public void Reset()
    {
        FullName = string.Empty;
        Position = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        HireDate = DateTimeOffset.Now;
        CreateUserAccount = false;
        Username = string.Empty;
        Password = string.Empty;
        SelectedRole = null;
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
            parsedHireDate,
            CreateUserAccount,
            string.IsNullOrWhiteSpace(Username) ? null : Username,
            string.IsNullOrWhiteSpace(Password) ? null : Password,
            SelectedRole?.Id
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
