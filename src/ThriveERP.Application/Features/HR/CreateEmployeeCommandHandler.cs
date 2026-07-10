namespace ThriveERP.Application.Features.HR;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Guid>
{
    private readonly IEmployeeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(IEmployeeRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            FullName = request.FullName,
            Position = request.Position,
            Phone = request.Phone,
            Email = request.Email,
            HireDate = request.HireDate,
            IsActive = true
        };

        await _repository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}
