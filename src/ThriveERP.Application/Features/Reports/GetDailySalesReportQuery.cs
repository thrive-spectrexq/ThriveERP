using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Enums;

namespace ThriveERP.Application.Features.Reports;

public record GetDailySalesReportQuery(DateTime Date) : IRequest<DailySalesReportDto>;

public class GetDailySalesReportQueryHandler : IRequestHandler<GetDailySalesReportQuery, DailySalesReportDto>
{
    private readonly ISalesOrderRepository _salesRepository;

    public GetDailySalesReportQueryHandler(ISalesOrderRepository salesRepository)
    {
        _salesRepository = salesRepository;
    }

    public async Task<DailySalesReportDto> Handle(GetDailySalesReportQuery request, CancellationToken cancellationToken)
    {
        var orders = await _salesRepository.GetAllAsync(cancellationToken);
        
        // Filter by date (ignoring time)
        var dailyOrders = orders
            .Where(o => o.OrderDate.Date == request.Date.Date && o.Status != OrderStatus.Voided)
            .ToList();

        return new DailySalesReportDto
        {
            Date = request.Date,
            TotalSales = dailyOrders.Sum(o => o.GrandTotal),
            OrderCount = dailyOrders.Count
        };
    }
}
