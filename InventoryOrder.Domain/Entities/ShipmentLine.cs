namespace InventoryOrder.Domain.Entities;

public class ShipmentLine
{
    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public Guid SalesOrderLineId { get; private set; }
    public decimal QuantityShipped { get; private set; }

    public ShipmentLine() { }

    private ShipmentLine(Guid shipmentId, Guid salesOrderLineId, decimal quantityShipped)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        SalesOrderLineId = salesOrderLineId;
        QuantityShipped = quantityShipped;
    }

    public static ShipmentLine Create(Guid shipmentId, Guid salesOrderLineId, decimal quantityShipped)
    {
        if (shipmentId == Guid.Empty)
            throw new ArgumentException("Shipment ID is required.", nameof(shipmentId));
        if (salesOrderLineId == Guid.Empty)
            throw new ArgumentException("Sales order line ID is required.", nameof(salesOrderLineId));
        if (quantityShipped <= 0)
            throw new ArgumentException("Shipped quantity must be positive.", nameof(quantityShipped));

        return new ShipmentLine(shipmentId, salesOrderLineId, quantityShipped);
    }
}
