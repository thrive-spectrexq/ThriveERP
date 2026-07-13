using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;

namespace ThriveERP.Application.Features.Settings;

public record UpdateSettingCommand(string Key, string Value) : IRequest;

public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand>
{
    private readonly ISystemSettingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSettingCommandHandler(ISystemSettingRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _repository.GetByKeyAsync(request.Key, cancellationToken);

        if (setting == null)
        {
            setting = new SystemSetting
            {
                Key = request.Key,
                Value = request.Value
            };
            await _repository.AddAsync(setting, cancellationToken);
        }
        else
        {
            setting.Value = request.Value;
            _repository.Update(setting);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
