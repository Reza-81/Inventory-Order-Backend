using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryOrder.Infrastructure.Persistence.Configurations;

public sealed class UserWarehouseConfiguration : IEntityTypeConfiguration<UserWarehouse>
{
    public void Configure(EntityTypeBuilder<UserWarehouse> builder)
    {
        builder.ToTable("UserWarehouses");
        builder.HasKey(assignment => new { assignment.UserId, assignment.WarehouseId });
        builder.HasOne(assignment => assignment.Warehouse)
            .WithMany()
            .HasForeignKey(assignment => assignment.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
