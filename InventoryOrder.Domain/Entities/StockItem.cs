namespace InventoryOrder.Domain.Entities;

public class StockItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal QuantityOnHand { get; private set; }
    public decimal QuantityReserved { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public StockItem() { }

    private StockItem(Guid productId, Guid warehouseId)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        WarehouseId = warehouseId;
    }

    public static StockItem Create(Guid productId, Guid warehouseId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.", nameof(productId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID is required.", nameof(warehouseId));

        return new StockItem(productId, warehouseId);
    }

    public void Reserve(decimal quantity)
    {
        ValidatePositive(quantity);
        if (QuantityOnHand - QuantityReserved < quantity)
            throw new InvalidOperationException("Insufficient available stock.");

        QuantityReserved += quantity;
    }

    public void Release(decimal quantity)
    {
        if (quantity <= 0 || quantity > QuantityReserved)
            throw new InvalidOperationException("Invalid release quantity.");

        QuantityReserved -= quantity;
    }

    public void Receive(decimal quantity)
    {
        ValidatePositive(quantity);
        QuantityOnHand += quantity;
    }

    public void Adjust(decimal quantity)
    {
        if (QuantityOnHand + quantity < QuantityReserved)
            throw new InvalidOperationException("Adjustment cannot reduce stock below the reserved quantity.");

        QuantityOnHand += quantity;
    }

    public void Ship(decimal quantity)
    {
        if (quantity <= 0 || quantity > QuantityReserved)
            throw new InvalidOperationException("Shipment quantity exceeds the reserved quantity.");

        QuantityOnHand -= quantity;
        QuantityReserved -= quantity;
    }

    private static void ValidatePositive(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
    }
}
