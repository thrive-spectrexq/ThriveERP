using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Accounting;

namespace ThriveERP.Desktop.ViewModels;

public partial class AddExpenseViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<AccountDto> _availableAccounts = new();

    [ObservableProperty]
    private AccountDto? _selectedAccount;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private DateTimeOffset _expenseDate = DateTimeOffset.Now;

    public Action? OnSaveComplete { get; set; }
    public Action? OnCancel { get; set; }

    public AddExpenseViewModel() { _mediator = null!; } // designer

    public AddExpenseViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadAccountsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAccountsAsync()
    {
        var accounts = await _mediator.Send(new GetAllAccountsQuery());
        AvailableAccounts = new ObservableCollection<AccountDto>(accounts);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedAccount == null || string.IsNullOrWhiteSpace(Category) || Amount <= 0)
            return;

        var command = new AddExpenseCommand(
            Category,
            Amount,
            SelectedAccount.Id,
            Description,
            DateOnly.FromDateTime(ExpenseDate.DateTime),
            Guid.Empty // Dummy user id for MVP
        );

        try
        {
            await _mediator.Send(command);
            OnSaveComplete?.Invoke();
        }
        catch (Exception)
        {
            // Error handling
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCancel?.Invoke();
    }
}
