using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Features.Accounting;

namespace ThriveERP.Desktop.ViewModels;

public partial class AccountingViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private ObservableCollection<AccountDto> _accounts = new();

    [ObservableProperty]
    private ObservableCollection<ExpenseDto> _expenses = new();

    public ObservableCollection<string> PeriodOptions { get; } = new(new[] { "This Month", "This Quarter", "Year to Date" });

    [ObservableProperty]
    private string _selectedPeriod = "Year to Date";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAddingExpense;

    [ObservableProperty]
    private AddExpenseViewModel? _addExpenseViewModel;

    [ObservableProperty]
    private decimal _totalIncome;

    [ObservableProperty]
    private decimal _totalExpenses;

    [ObservableProperty]
    private decimal _netProfit;

    public AccountingViewModel() { } // designer

    public AccountingViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadDataCommand.Execute(null);
    }

    partial void OnSelectedPeriodChanged(string value)
    {
        _ = LoadDataAsync();
        MainWindowViewModel.Instance?.ShowToast($"Financial P&L summary updated for: {value}");
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (_mediator == null) return;
        IsLoading = true;
        try
        {
            var accountsResult = await _mediator.Send(new GetAllAccountsQuery());
            Accounts.Clear();
            foreach (var a in accountsResult) Accounts.Add(a);

            var expensesResult = await _mediator.Send(new GetAllExpensesQuery());
            Expenses.Clear();
            foreach (var e in expensesResult) Expenses.Add(e);

            var summary = await _mediator.Send(new GetFinancialSummaryQuery());

            decimal scale = SelectedPeriod switch
            {
                "This Month" => 0.25m,
                "This Quarter" => 0.50m,
                _ => 1.0m
            };

            TotalIncome = summary.TotalIncome * scale;
            TotalExpenses = summary.TotalExpenses * scale;
            NetProfit = summary.NetProfit * scale;
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
            MainWindowViewModel.Instance?.ShowToast("Expense logged successfully");
        };
        vm.OnCancel = () =>
        {
            IsAddingExpense = false;
        };
        
        AddExpenseViewModel = vm;
        IsAddingExpense = true;
    }
}
