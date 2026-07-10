namespace ThriveERP.Application.Features.HR;
using MediatR;
using System.Collections.Generic;

public record GetAllEmployeesQuery : IRequest<IReadOnlyList<EmployeeDto>>;
