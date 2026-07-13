using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ThriveERP.Application.Features.Accounting;

namespace ThriveERP.Desktop.ViewModels;

public partial class AccountingViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private ObservableCollection<AccountDto> _accounts = new();

    [ObservableProperty]
    private ObservableCollection<ExpenseDto> _expenses = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAddingExpense;

    [ObservableProperty]
    private AddExpenseViewModel? _addExpenseViewModel;

    public AccountingViewModel() { } // designer

    public AccountingViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var accountsResult = await _mediator.Send(new GetAllAccountsQuery());
            Accounts.Clear();
            foreach (var a in accountsResult) Accounts.Add(a);

            var expensesResult = await _mediator.Send(new GetAllExpensesQuery());
            Expenses.Clear();
            foreach (var e in expensesResult) Expenses.Add(e);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowAddExpense()
    {
        var vm = App.Services!.GetRequiredService<AddExpenseViewModel>();
        vm.OnSaveComplete = () =>
        {
            IsAddingExpense = false;
            LoadDataCommand.Execute(null);
        };
        vm.OnCancel = () =>
        {
            IsAddingExpense = false;
        };
        
        AddExpenseViewModel = vm;
        IsAddingExpense = true;
    }
}
