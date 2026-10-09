using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.OrderDate).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
