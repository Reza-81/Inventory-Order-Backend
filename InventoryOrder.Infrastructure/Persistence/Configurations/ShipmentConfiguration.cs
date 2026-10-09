using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ShipmentNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.ShipmentNumber).IsUnique();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Carrier).HasMaxLength(100);
        builder.Property(x => x.TrackingNumber).HasMaxLength(200);
        builder.HasOne<SalesOrder>().WithMany().HasForeignKey(x => x.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
