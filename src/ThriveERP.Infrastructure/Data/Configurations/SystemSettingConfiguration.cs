using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThriveERP.Domain.Entities;

namespace ThriveERP.Infrastructure.Data.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Value)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.Key).IsUnique();
    }
}
