using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class ShipmentLineConfiguration : IEntityTypeConfiguration<ShipmentLine>
{
    public void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        builder.ToTable("ShipmentLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityShipped).HasPrecision(18, 3).IsRequired();
        builder.HasOne<SalesOrderLine>().WithMany().HasForeignKey(x => x.SalesOrderLineId).OnDelete(DeleteBehavior.Restrict);
    }
}
