using System;

namespace InventoryOrder.Domain.Entities;

public class StockItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }

    public decimal QuantityOnHand { get; private set; }
    public decimal QuantityReserved { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    /// <summary>
    /// For EF Core
    /// </summary>
    public StockItem() { }

    private StockItem(Guid productId, Guid warehouseId)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        QuantityOnHand = 0;
        QuantityReserved = 0;
    }
    
    /// <summary>
    /// Factory Method
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="warehouseId"></param>
    /// <returns></returns>
    public static StockItem Create(Guid productId, Guid warehouseId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.");

        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID is required.");

        return new StockItem(productId, warehouseId);
    }

    public void Reserve(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");

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
}
