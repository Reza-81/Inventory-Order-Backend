using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityOrdered).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.QuantityShipped).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Ignore(x => x.LineTotal);
        builder.Ignore(x => x.IsFullyShipped);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}
