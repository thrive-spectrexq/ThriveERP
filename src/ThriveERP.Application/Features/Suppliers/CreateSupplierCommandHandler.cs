namespace ThriveERP.Application.Features.Suppliers;
using AutoMapper;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierDto>
{
    private readonly IRepository<Supplier> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSupplierCommandHandler(IRepository<Supplier> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SupplierDto> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = new Supplier 
        { 
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            IsActive = request.IsActive
        };
        await _repository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new SupplierDto { 
            Id = supplier.Id, 
            Name = supplier.Name,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            CurrentBalance = supplier.CurrentBalance,
            IsActive = supplier.IsActive
        };
    }
}
