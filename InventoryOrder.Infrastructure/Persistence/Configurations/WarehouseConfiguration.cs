using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Line1).HasColumnName("AddressLine1").HasMaxLength(200).IsRequired();
            address.Property(x => x.Line2).HasColumnName("AddressLine2").HasMaxLength(200);
            address.Property(x => x.City).HasColumnName("City").HasMaxLength(100).IsRequired();
            address.Property(x => x.State).HasColumnName("State").HasMaxLength(100);
            address.Property(x => x.PostalCode).HasColumnName("PostalCode").HasMaxLength(30).IsRequired();
            address.Property(x => x.Country).HasColumnName("Country").HasMaxLength(100).IsRequired();
        });
        builder.Navigation(x => x.Address).IsRequired();
    }
}
