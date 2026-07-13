namespace ThriveERP.Application.Features.Sales;

using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ThriveERP.Application.Common.Interfaces;

public class GetSalesOrderByIdQueryHandler : IRequestHandler<GetSalesOrderByIdQuery, SalesOrderDto?>
{
    private readonly ISalesOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetSalesOrderByIdQueryHandler(ISalesOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SalesOrderDto?> Handle(GetSalesOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetWithItemsAsync(request.Id);
        if (entity == null) return null;
        
        return _mapper.Map<SalesOrderDto>(entity);
    }
}
