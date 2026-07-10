namespace ThriveERP.Application.Common.Interfaces;

/// <summary>
/// Provides an abstraction over the system clock for testability.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>Gets the current UTC date and time.</summary>
    DateTime UtcNow { get; }
}
