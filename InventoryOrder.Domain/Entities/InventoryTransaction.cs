using InventoryOrder.Domain.Enums;

namespace InventoryOrder.Domain.Entities;

public class InventoryTransaction
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid StockItemId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public string? Reason { get; private set; }
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public Guid? ActorId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    public InventoryTransaction() { }

    private InventoryTransaction(Guid productId, Guid warehouseId, Guid stockItemId, TransactionType type, decimal quantity, string? reason, string? referenceType, Guid? referenceId, Guid? actorId)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        WarehouseId = warehouseId;
        StockItemId = stockItemId;
        Type = type;
        Quantity = quantity;
        Reason = reason?.Trim();
        ReferenceType = referenceType?.Trim();
        ReferenceId = referenceId;
        ActorId = actorId;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public static InventoryTransaction Create(Guid productId, Guid warehouseId, Guid stockItemId, TransactionType type, decimal quantity, string? reason = null, string? referenceType = null, Guid? referenceId = null, Guid? actorId = null)
    {
        if (productId == Guid.Empty || warehouseId == Guid.Empty || stockItemId == Guid.Empty)
            throw new ArgumentException("Product, warehouse, and stock item IDs are required.");
        if (!Enum.IsDefined(type))
            throw new ArgumentException("Transaction type is invalid.", nameof(type));
        if (type == TransactionType.Adjust ? quantity == 0 : quantity <= 0)
            throw new ArgumentException(
                type == TransactionType.Adjust
                    ? "Adjustment quantity cannot be zero."
                    : "Quantity must be positive.",
                nameof(quantity));

        return new InventoryTransaction(productId, warehouseId, stockItemId, type, quantity, reason, referenceType, referenceId, actorId);
    }
}
