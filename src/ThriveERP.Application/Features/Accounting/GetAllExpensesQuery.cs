namespace ThriveERP.Application.Features.Accounting;

using MediatR;
using ThriveERP.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetAllExpensesQuery : IRequest<IEnumerable<ExpenseDto>>;

public class GetAllExpensesQueryHandler : IRequestHandler<GetAllExpensesQuery, IEnumerable<ExpenseDto>>
{
    private readonly IExpenseRepository _repository;

    public GetAllExpensesQueryHandler(IExpenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ExpenseDto>> Handle(GetAllExpensesQuery request, CancellationToken cancellationToken)
    {
        var expenses = await _repository.GetAllAsync(cancellationToken);
        return expenses.Select(e => new ExpenseDto(
            e.Id,
            e.Category,
            e.Amount,
            e.AccountId,
            e.Description,
            e.ExpenseDate,
            e.RecordedByUserId
        ));
    }
}
