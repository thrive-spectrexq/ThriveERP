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
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private string _title = "Human Resources";

    [ObservableProperty]
    private ObservableCollection<EmployeeDto> _employees = new();

    [ObservableProperty]
    private ObservableCollection<EmployeeDto> _filteredEmployees = new();

    [ObservableProperty]
    private EmployeeDto? _selectedEmployee;

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
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredEmployees = new ObservableCollection<EmployeeDto>(Employees);
        }
        else
        {
            var q = SearchQuery.ToLower();
            FilteredEmployees = new ObservableCollection<EmployeeDto>(
                Employees.Where(e => e.FullName.ToLower().Contains(q) || 
                                    (e.Email != null && e.Email.ToLower().Contains(q)) ||
                                    (e.Position != null && e.Position.ToLower().Contains(q))));
        }
    }

    [RelayCommand]
    private void ShowAddEmployee()
    {
        AddEmployeeViewModel.Reset();
        IsAddingEmployee = true;
    }
}
