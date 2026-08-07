using ThriveERP.Application.Common.Interfaces;

namespace ThriveERP.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
