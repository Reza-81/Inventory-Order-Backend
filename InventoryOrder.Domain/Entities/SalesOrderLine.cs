namespace InventoryOrder.Domain.Entities;

public class SalesOrderLine
{
    public Guid Id { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal QuantityOrdered { get; private set; }
    public decimal QuantityShipped { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = null!;
    public decimal LineTotal => QuantityOrdered * UnitPrice;
    public bool IsFullyShipped => QuantityShipped == QuantityOrdered;

    public SalesOrderLine() { }

    private SalesOrderLine(Guid salesOrderId, Guid productId, Guid warehouseId, decimal quantityOrdered, decimal unitPrice, string currency)
    {
        Id = Guid.NewGuid();
        SalesOrderId = salesOrderId;
        ProductId = productId;
        WarehouseId = warehouseId;
        QuantityOrdered = quantityOrdered;
        UnitPrice = unitPrice;
        Currency = currency;
    }

    public static SalesOrderLine Create(Guid salesOrderId, Guid productId, Guid warehouseId, decimal quantityOrdered, decimal unitPrice, string currency = "USD")
    {
        if (salesOrderId == Guid.Empty || productId == Guid.Empty || warehouseId == Guid.Empty)
            throw new ArgumentException("Order, product, and warehouse IDs are required.");
        if (quantityOrdered <= 0)
            throw new ArgumentException("Ordered quantity must be positive.", nameof(quantityOrdered));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        return new SalesOrderLine(salesOrderId, productId, warehouseId, quantityOrdered, unitPrice, currency.Trim().ToUpperInvariant());
    }

    public void Ship(decimal quantity)
    {
        if (quantity <= 0 || QuantityShipped + quantity > QuantityOrdered)
            throw new InvalidOperationException("Shipment quantity exceeds the ordered quantity.");
        QuantityShipped += quantity;
    }
}
