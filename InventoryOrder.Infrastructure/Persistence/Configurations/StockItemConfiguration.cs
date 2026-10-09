using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityOnHand).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.QuantityReserved).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.RowVersion)
            .HasColumnType("binary(16)")
            .HasMaxLength(16)
            .IsConcurrencyToken()
            .ValueGeneratedNever();
        builder.HasIndex(x => new { x.ProductId, x.WarehouseId }).IsUnique();
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}
