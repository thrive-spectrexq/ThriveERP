namespace ThriveERP.Application.Features.Accounting;

using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;

public record FinancialSummaryDto(decimal TotalIncome, decimal TotalExpenses, decimal NetProfit);

public record GetFinancialSummaryQuery : IRequest<FinancialSummaryDto>;

public class GetFinancialSummaryQueryHandler : IRequestHandler<GetFinancialSummaryQuery, FinancialSummaryDto>
{
    private readonly ISalesOrderRepository _salesRepository;
    private readonly IRepository<Expense> _expenseRepository;

    public GetFinancialSummaryQueryHandler(
        ISalesOrderRepository salesRepository,
        IRepository<Expense> expenseRepository)
    {
        _salesRepository = salesRepository;
        _expenseRepository = expenseRepository;
    }

    public async Task<FinancialSummaryDto> Handle(GetFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        var allSales = await _salesRepository.GetAllAsync(cancellationToken);
        var activeSales = allSales.Where(x => x.Status != ThriveERP.Domain.Enums.OrderStatus.Voided).ToList();
        
        var allExpenses = await _expenseRepository.GetAllAsync(cancellationToken);

        decimal totalIncome = activeSales.Sum(x => x.GrandTotal);
        decimal totalExpenses = allExpenses.Sum(x => x.Amount);
        decimal netProfit = totalIncome - totalExpenses;

        return new FinancialSummaryDto(totalIncome, totalExpenses, netProfit);
    }
}
