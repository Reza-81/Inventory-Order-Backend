using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.EntityName).HasMaxLength(200).IsRequired();
        builder.Property(log => log.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(log => log.Action).HasMaxLength(30).IsRequired();
        builder.Property(log => log.OldValuesJson).HasColumnType("longtext");
        builder.Property(log => log.NewValuesJson).HasColumnType("longtext");
        builder.Property(log => log.Timestamp).IsRequired();
        builder.HasIndex(log => new { log.EntityName, log.EntityId });
        builder.HasIndex(log => log.Timestamp);
    }
}
