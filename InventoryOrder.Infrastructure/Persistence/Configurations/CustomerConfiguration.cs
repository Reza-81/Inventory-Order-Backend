using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        
        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.Phone)
            .HasMaxLength(20);
        
        builder.OwnsOne(customer => customer.BillingAddress, address =>
        {
            address.Property(value => value.Line1).HasColumnName("BillingLine1").HasMaxLength(200).IsRequired();
            address.Property(value => value.Line2).HasColumnName("BillingLine2").HasMaxLength(200);
            address.Property(value => value.City).HasColumnName("BillingCity").HasMaxLength(100).IsRequired();
            address.Property(value => value.State).HasColumnName("BillingState").HasMaxLength(100);
            address.Property(value => value.PostalCode).HasColumnName("BillingPostalCode").HasMaxLength(30).IsRequired();
            address.Property(value => value.Country).HasColumnName("BillingCountry").HasMaxLength(100).IsRequired();
        });

        builder.Navigation(customer => customer.BillingAddress).IsRequired();

        builder.OwnsOne(customer => customer.ShippingAddress, address =>
        {
            address.Property(value => value.Line1).HasColumnName("ShippingLine1").HasMaxLength(200);
            address.Property(value => value.Line2).HasColumnName("ShippingLine2").HasMaxLength(200);
            address.Property(value => value.City).HasColumnName("ShippingCity").HasMaxLength(100);
            address.Property(value => value.State).HasColumnName("ShippingState").HasMaxLength(100);
            address.Property(value => value.PostalCode).HasColumnName("ShippingPostalCode").HasMaxLength(30);
            address.Property(value => value.Country).HasColumnName("ShippingCountry").HasMaxLength(100);
        });

        builder.Navigation(customer => customer.ShippingAddress).IsRequired(false);

        builder.Property(customer => customer.IsActive)
            .IsRequired();

    }
}
