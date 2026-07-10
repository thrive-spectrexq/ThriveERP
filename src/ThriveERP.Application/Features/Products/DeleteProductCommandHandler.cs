using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Domain.Exceptions;

namespace ThriveERP.Application.Features.Products;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            throw new EntityNotFoundException(nameof(Product), request.Id);

        _repository.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
