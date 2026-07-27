using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Features.Returns;

public record GetAllReturnsQuery : IRequest<List<ReturnDto>>;

public class GetAllReturnsQueryHandler : IRequestHandler<GetAllReturnsQuery, List<ReturnDto>>
{
    private readonly IRepository<Return> _returnRepository;
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly IProductRepository _productRepository;

    public GetAllReturnsQueryHandler(
        IRepository<Return> returnRepository,
        ISalesOrderRepository salesOrderRepository,
        IProductRepository productRepository)
    {
        _returnRepository = returnRepository;
        _salesOrderRepository = salesOrderRepository;
        _productRepository = productRepository;
    }

    public async Task<List<ReturnDto>> Handle(GetAllReturnsQuery request, CancellationToken cancellationToken)
    {
        var returns = await _returnRepository.GetAllAsync(cancellationToken);
        var orders = await _salesOrderRepository.GetAllAsync(cancellationToken);
        var products = await _productRepository.GetAllAsync(cancellationToken);

        var orderDict = orders.ToDictionary(o => o.Id, o => o.OrderNumber);
        var productDict = products.ToDictionary(p => p.Id, p => p.Name);

        var list = new List<ReturnDto>();
        foreach (var r in returns.OrderByDescending(x => x.ProcessedAtUtc))
        {
            list.Add(new ReturnDto
            {
                Id = r.Id,
                SalesOrderId = r.SalesOrderId,
                OrderNumber = orderDict.TryGetValue(r.SalesOrderId, out var orderNum) ? orderNum : "SO-UNKNOWN",
                ProductId = r.ProductId,
                ProductName = productDict.TryGetValue(r.ProductId, out var prodName) ? prodName : "Returned Product",
                Quantity = r.Quantity,
                RefundAmount = r.RefundAmount,
                Reason = string.IsNullOrWhiteSpace(r.Reason) ? "Customer Refund Request" : r.Reason,
                ProcessedAtUtc = r.ProcessedAtUtc
            });
        }

        return list;
    }
}
