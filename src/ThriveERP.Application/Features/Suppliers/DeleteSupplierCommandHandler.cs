namespace ThriveERP.Application.Features.Suppliers;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand>
{
    private readonly ISupplierRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSupplierCommandHandler(ISupplierRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier != null)
        {
            _repository.Delete(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
