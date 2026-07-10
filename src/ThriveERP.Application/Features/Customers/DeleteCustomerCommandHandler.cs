using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Domain.Exceptions;

namespace ThriveERP.Application.Features.Customers;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Customer), request.Id);

        _repository.Delete(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
