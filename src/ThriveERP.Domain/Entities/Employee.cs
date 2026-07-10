namespace ThriveERP.Domain.Entities;

using ThriveERP.Domain.Common;

public class Employee : BaseEntity, IAggregateRoot
{
    public Guid? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateOnly? HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
}
