using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Application.Features.Settings;

public record GetSettingQuery(string Key) : IRequest<string?>;

public class GetSettingQueryHandler : IRequestHandler<GetSettingQuery, string?>
{
    private readonly ISystemSettingRepository _repository;

    public GetSettingQueryHandler(ISystemSettingRepository repository)
    {
        _repository = repository;
    }

    public async Task<string?> Handle(GetSettingQuery request, CancellationToken cancellationToken)
    {
        var setting = await _repository.GetByKeyAsync(request.Key, cancellationToken);
        return setting?.Value;
    }
}
