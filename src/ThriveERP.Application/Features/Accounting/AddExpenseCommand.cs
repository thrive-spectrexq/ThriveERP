namespace ThriveERP.Application.Features.Accounting;

using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

public record AddExpenseCommand(
    string Category,
    decimal Amount,
    Guid AccountId,
    string? Description,
    DateOnly ExpenseDate,
    Guid RecordedByUserId
) : IRequest<Guid>;

public class AddExpenseCommandHandler : IRequestHandler<AddExpenseCommand, Guid>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddExpenseCommandHandler(IExpenseRepository expenseRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _expenseRepository = expenseRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(AddExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Category = request.Category,
            Amount = request.Amount,
            AccountId = request.AccountId,
            Description = request.Description,
            ExpenseDate = request.ExpenseDate,
            RecordedByUserId = request.RecordedByUserId
        };

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account != null)
        {
            account.Balance -= request.Amount; 
            _accountRepository.Update(account);
        }

        await _expenseRepository.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return expense.Id;
    }
}
