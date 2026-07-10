namespace ThriveERP.Application.Features.Accounting;

using MediatR;
using ThriveERP.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetAllAccountsQuery : IRequest<IEnumerable<AccountDto>>;

public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, IEnumerable<AccountDto>>
{
    private readonly IAccountRepository _repository;

    public GetAllAccountsQueryHandler(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _repository.GetAllAsync(cancellationToken);
        return accounts.Select(a => new AccountDto(
            a.Id,
            a.Name,
            a.Type,
            a.Balance,
            a.IsActive
        ));
    }
}
