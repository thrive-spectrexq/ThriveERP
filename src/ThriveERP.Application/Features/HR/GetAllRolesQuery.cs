namespace ThriveERP.Application.Features.HR;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;

public record GetAllRolesQuery() : IRequest<IReadOnlyList<RoleDto>>;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IRoleRepository _repository;

    public GetAllRolesQueryHandler(IRoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _repository.GetAllAsync(cancellationToken);
        return roles.Select(r => new RoleDto(r.Id, r.Name)).ToList();
    }
}
