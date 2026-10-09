using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.UnitOfMeasure)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(product => product.ReorderLevel)
            .HasPrecision(18, 3);

        builder.Property(product => product.UpdatedAt)
            .IsRequired();

        builder.OwnsOne(product => product.Sku, sku =>
        {
            sku.Property(value => value.Value)
                .HasColumnName("Sku")
                .HasMaxLength(50)
                .IsRequired();

            sku.HasIndex(value => value.Value)
                .IsUnique();
        });

        builder.OwnsOne(product => product.Price, price =>
        {
            price.Property(value => value.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2);

            price.Property(value => value.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });
    }
}