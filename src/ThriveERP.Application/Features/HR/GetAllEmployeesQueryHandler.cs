namespace ThriveERP.Application.Features.HR;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;

public class GetAllEmployeesQueryHandler : IRequestHandler<GetAllEmployeesQuery, IReadOnlyList<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;

    public GetAllEmployeesQueryHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<EmployeeDto>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await _repository.GetAllAsync(cancellationToken);
        return employees.Select(e => new EmployeeDto(
            e.Id,
            e.UserId,
            e.FullName,
            e.Position,
            e.Phone,
            e.Email,
            e.HireDate,
            e.IsActive
        )).ToList();
    }
}
