namespace ThriveERP.Application.Features.Suppliers;
using AutoMapper;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
public class GetAllSuppliersQueryHandler : IRequestHandler<GetAllSuppliersQuery, List<SupplierDto>>
{
    private readonly IRepository<Supplier> _repository;
    private readonly IMapper _mapper;
    public GetAllSuppliersQueryHandler(IRepository<Supplier> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<SupplierDto>> Handle(GetAllSuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await _repository.GetAllAsync(cancellationToken);
        return suppliers.Select(s => new SupplierDto 
        { 
            Id = s.Id, 
            Name = s.Name,
            Phone = s.Phone,
            Email = s.Email,
            Address = s.Address,
            CurrentBalance = s.CurrentBalance,
            IsActive = s.IsActive
        }).ToList();
    }
}
