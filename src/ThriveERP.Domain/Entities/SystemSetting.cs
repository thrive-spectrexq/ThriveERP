using System.ComponentModel.DataAnnotations;
using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

public class SystemSetting : BaseEntity
{
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Value { get; set; } = string.Empty;
}
