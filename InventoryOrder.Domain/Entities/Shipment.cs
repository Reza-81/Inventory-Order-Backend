using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.Common;

namespace InventoryOrder.Domain.Entities;

public class Shipment : IAuditable
{
    private readonly List<ShipmentLine> _lines = [];

    public Guid Id { get; private set; }
    public string ShipmentNumber { get; private set; } = null!;
    public Guid SalesOrderId { get; private set; }
    public DateTimeOffset? ShippedAt { get; private set; }
    public string? Carrier { get; private set; }
    public string? TrackingNumber { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public IReadOnlyCollection<ShipmentLine> Lines => _lines.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public Shipment() { }

    private Shipment(string shipmentNumber, Guid salesOrderId, string? carrier, string? trackingNumber)
    {
        Id = Guid.NewGuid();
        ShipmentNumber = shipmentNumber;
        SalesOrderId = salesOrderId;
        Carrier = carrier?.Trim();
        TrackingNumber = trackingNumber?.Trim();
        Status = ShipmentStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Shipment Create(string shipmentNumber, Guid salesOrderId, string? carrier = null, string? trackingNumber = null)
    {
        if (string.IsNullOrWhiteSpace(shipmentNumber))
            throw new ArgumentException("Shipment number is required.", nameof(shipmentNumber));
        if (salesOrderId == Guid.Empty)
            throw new ArgumentException("Sales order ID is required.", nameof(salesOrderId));

        return new Shipment(shipmentNumber.Trim(), salesOrderId, carrier, trackingNumber);
    }

    public void AddLine(ShipmentLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (Status != ShipmentStatus.Pending)
            throw new InvalidOperationException("Lines can only be added to a pending shipment.");
        if (line.ShipmentId != Id)
            throw new InvalidOperationException("The shipment line belongs to a different shipment.");
        _lines.Add(line);
    }

    public void SetCreatedAudit(DateTimeOffset timestamp, Guid? actorId)
    {
        CreatedAt = timestamp;
        CreatedBy = actorId;
        UpdatedAt = timestamp;
        UpdatedBy = actorId;
    }

    public void SetUpdatedAudit(DateTimeOffset timestamp, Guid? actorId)
    {
        UpdatedAt = timestamp;
        UpdatedBy = actorId;
    }

    public void MarkShipped()
    {
        if (Status != ShipmentStatus.Pending)
            throw new InvalidOperationException("Only a pending shipment can be shipped.");
        if (_lines.Count == 0)
            throw new InvalidOperationException("A shipment must contain at least one line.");

        Status = ShipmentStatus.Shipped;
        ShippedAt = DateTimeOffset.UtcNow;
    }
}
