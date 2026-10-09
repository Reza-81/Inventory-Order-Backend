using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");
        builder.HasKey(record => record.Key);
        builder.Property(record => record.Key).HasMaxLength(200).IsRequired();
        builder.Property(record => record.RequestHash).HasMaxLength(64).IsRequired();
        builder.Property(record => record.ResponseJson).HasColumnType("longtext").IsRequired();
        builder.Property(record => record.CreatedAt).IsRequired();
        builder.Property(record => record.ExpiresAt).IsRequired();
        builder.HasIndex(record => record.ExpiresAt);
    }
}
