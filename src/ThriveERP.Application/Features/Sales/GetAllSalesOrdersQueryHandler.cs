namespace ThriveERP.Application.Features.Sales;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;

public class GetAllSalesOrdersQueryHandler : IRequestHandler<GetAllSalesOrdersQuery, List<SalesOrderDto>>
{
    private readonly IRepository<SalesOrder> _repository;
    private readonly IMapper _mapper;

    public GetAllSalesOrdersQueryHandler(IRepository<SalesOrder> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<SalesOrderDto>> Handle(GetAllSalesOrdersQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<SalesOrderDto>>(entities);
    }
}
