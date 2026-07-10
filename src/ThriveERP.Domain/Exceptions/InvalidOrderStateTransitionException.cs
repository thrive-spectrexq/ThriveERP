namespace ThriveERP.Domain.Exceptions;

/// <summary>
/// Thrown when an order status transition violates the allowed state machine.
/// </summary>
public class InvalidOrderStateTransitionException : Exception
{
    public string CurrentStatus { get; }
    public string AttemptedStatus { get; }

    public InvalidOrderStateTransitionException(string currentStatus, string attemptedStatus)
        : base($"Cannot transition order from '{currentStatus}' to '{attemptedStatus}'.")
    {
        CurrentStatus = currentStatus;
        AttemptedStatus = attemptedStatus;
    }

    public InvalidOrderStateTransitionException() : base("Invalid order state transition.") { }

    public InvalidOrderStateTransitionException(string message) : base(message) { }

    public InvalidOrderStateTransitionException(string message, Exception innerException)
        : base(message, innerException) { }
}
