namespace InventoryOrder.Domain.Entities;

public sealed class UserWarehouse
{
    public Guid UserId { get; private set; }
    public Guid WarehouseId { get; private set; }

    public ApplicationUser User { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;

    public UserWarehouse() { }

    private UserWarehouse(Guid userId, Guid warehouseId)
    {
        UserId = userId;
        WarehouseId = warehouseId;
    }

    public static UserWarehouse Create(Guid userId, Guid warehouseId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID is required.", nameof(warehouseId));

        return new UserWarehouse(userId, warehouseId);
    }
}
