using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Domain.Exceptions;

namespace ThriveERP.Application.Features.Customers;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Customer), request.Id);

        customer.Name = request.Name;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.Address = request.Address;
        customer.CreditLimit = request.CreditLimit;
        customer.IsActive = request.IsActive;

        _repository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomerDto>(customer);
    }
}
