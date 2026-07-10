namespace ThriveERP.Application.Features.Customers;
using AutoMapper;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    private readonly IRepository<Customer> _repository;
    private readonly IMapper _mapper;
    public GetAllCustomersQueryHandler(IRepository<Customer> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<CustomerDto>>(list);
    }
}
