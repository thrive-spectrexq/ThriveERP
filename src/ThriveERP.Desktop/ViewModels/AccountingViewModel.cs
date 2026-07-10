using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
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

    public AccountingViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadDataAsync().ConfigureAwait(false);
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
}
