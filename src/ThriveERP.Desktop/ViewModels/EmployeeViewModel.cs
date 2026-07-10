using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ThriveERP.Application.Features.HR;
using System.Linq;
using System;

namespace ThriveERP.Desktop.ViewModels;

public partial class EmployeeViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<EmployeeDto> _employees = new();

    [ObservableProperty]
    private bool _isAddingEmployee;

    public AddEmployeeViewModel AddEmployeeViewModel { get; }

    public EmployeeViewModel(IMediator mediator)
    {
        _mediator = mediator;
        AddEmployeeViewModel = new AddEmployeeViewModel(mediator);
        AddEmployeeViewModel.OnSave = async () =>
        {
            IsAddingEmployee = false;
            await LoadEmployeesAsync();
        };
        AddEmployeeViewModel.OnCancel = () =>
        {
            IsAddingEmployee = false;
        };

        _ = LoadEmployeesAsync();
    }

    [RelayCommand]
    private async Task LoadEmployeesAsync()
    {
        var result = await _mediator.Send(new GetAllEmployeesQuery());
        Employees = new ObservableCollection<EmployeeDto>(result);
    }

    [RelayCommand]
    private void ShowAddEmployee()
    {
        AddEmployeeViewModel.Reset();
        IsAddingEmployee = true;
    }
}
