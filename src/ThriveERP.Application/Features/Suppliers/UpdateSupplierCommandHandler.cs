namespace ThriveERP.Application.Features.Suppliers;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand>
{
    private readonly ISupplierRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSupplierCommandHandler(ISupplierRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier == null) throw new System.Exception("Supplier not found");

        supplier.Name = request.Name;
        supplier.Phone = request.Phone;
        supplier.Email = request.Email;
        supplier.Address = request.Address;
        supplier.IsActive = request.IsActive;

        _repository.Update(supplier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
