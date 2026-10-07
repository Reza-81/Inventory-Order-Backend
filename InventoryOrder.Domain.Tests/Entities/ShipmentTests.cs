using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using Xunit;

namespace InventoryOrder.Domain.Tests.Entities;

public class ShipmentTests
{
    [Fact]
    public void Create_WithValidValues_CreatesPendingShipment()
    {
        var salesOrderId = Guid.NewGuid();

        var shipment = Shipment.Create(
            " shipment-001 ",
            salesOrderId,
            " UPS ",
            " TRACK-123 ");

        Assert.NotEqual(Guid.Empty, shipment.Id);
        Assert.Equal("shipment-001", shipment.ShipmentNumber);
        Assert.Equal(salesOrderId, shipment.SalesOrderId);
        Assert.Equal("UPS", shipment.Carrier);
        Assert.Equal("TRACK-123", shipment.TrackingNumber);
        Assert.Equal(ShipmentStatus.Pending, shipment.Status);
        Assert.Null(shipment.ShippedAt);
        Assert.Empty(shipment.Lines);
    }

    [Fact]
    public void Create_WithEmptyShipmentNumber_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Shipment.Create("", Guid.NewGuid()));

        Assert.Equal("shipmentNumber", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptySalesOrderId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Shipment.Create("SHIPMENT-001", Guid.Empty));

        Assert.Equal("salesOrderId", exception.ParamName);
    }

    [Fact]
    public void AddLine_WithMatchingShipment_AddsLine()
    {
        var shipment = Shipment.Create("SHIPMENT-001", Guid.NewGuid());
        var line = CreateLine(shipment);

        shipment.AddLine(line);

        Assert.Single(shipment.Lines);
        Assert.Same(line, shipment.Lines.Single());
    }

    [Fact]
    public void AddLine_ForDifferentShipment_ThrowsInvalidOperationException()
    {
        var shipment = Shipment.Create("SHIPMENT-001", Guid.NewGuid());
        var differentShipment = Shipment.Create("SHIPMENT-002", Guid.NewGuid());
        var line = CreateLine(differentShipment);

        var exception = Assert.Throws<InvalidOperationException>(() => shipment.AddLine(line));

        Assert.Equal("The shipment line belongs to a different shipment.", exception.Message);
    }

    [Fact]
    public void MarkShipped_WithLines_ChangesStatusAndSetsShippedAt()
    {
        var shipment = Shipment.Create("SHIPMENT-001", Guid.NewGuid());
        shipment.AddLine(CreateLine(shipment));

        shipment.MarkShipped();

        Assert.Equal(ShipmentStatus.Shipped, shipment.Status);
        Assert.NotNull(shipment.ShippedAt);
        Assert.True(shipment.ShippedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void MarkShipped_WithoutLines_ThrowsInvalidOperationException()
    {
        var shipment = Shipment.Create("SHIPMENT-001", Guid.NewGuid());

        var exception = Assert.Throws<InvalidOperationException>(() => shipment.MarkShipped());

        Assert.Equal("A shipment must contain at least one line.", exception.Message);
    }

    [Fact]
    public void MarkShipped_WhenAlreadyShipped_ThrowsInvalidOperationException()
    {
        var shipment = Shipment.Create("SHIPMENT-001", Guid.NewGuid());
        shipment.AddLine(CreateLine(shipment));
        shipment.MarkShipped();

        var exception = Assert.Throws<InvalidOperationException>(() => shipment.MarkShipped());

        Assert.Equal("Only a pending shipment can be shipped.", exception.Message);
    }

    [Fact]
    public void ShipmentLine_Create_WithValidValues_CreatesLine()
    {
        var shipmentId = Guid.NewGuid();
        var salesOrderLineId = Guid.NewGuid();

        var line = ShipmentLine.Create(shipmentId, salesOrderLineId, 3);

        Assert.NotEqual(Guid.Empty, line.Id);
        Assert.Equal(shipmentId, line.ShipmentId);
        Assert.Equal(salesOrderLineId, line.SalesOrderLineId);
        Assert.Equal(3, line.QuantityShipped);
    }

    [Fact]
    public void ShipmentLine_Create_WithNonPositiveQuantity_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ShipmentLine.Create(Guid.NewGuid(), Guid.NewGuid(), 0));

        Assert.Equal("quantityShipped", exception.ParamName);
    }

    private static ShipmentLine CreateLine(Shipment shipment)
    {
        return ShipmentLine.Create(
            shipment.Id,
            Guid.NewGuid(),
            quantityShipped: 2);
    }
}
