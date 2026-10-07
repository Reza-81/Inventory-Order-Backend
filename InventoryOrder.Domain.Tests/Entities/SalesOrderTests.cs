using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using Xunit;

namespace InventoryOrder.Domain.Tests.Entities;

public class SalesOrderTests
{
    [Fact]
    public void Create_WithValidValues_CreatesSalesOrder()
    {
        var customerId = Guid.NewGuid();

        var salesOrder = SalesOrder.Create(
            " order-001 ",
            customerId,
            " rial ");

        Assert.NotEqual(Guid.Empty, salesOrder.Id);
        Assert.Equal("order-001", salesOrder.OrderNumber);
        Assert.Equal(customerId, salesOrder.CustomerId);
        Assert.Equal(OrderStatus.Draft, salesOrder.Status);
        Assert.Equal("RIAL", salesOrder.Currency);
        Assert.Equal(0, salesOrder.TotalAmount);
        Assert.Empty(salesOrder.Lines);
        Assert.True(salesOrder.OrderDate <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Create_WithEmptyOrderNumber_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            SalesOrder.Create("", Guid.NewGuid()));

        Assert.Equal("orderNumber", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            SalesOrder.Create("ORDER-001", Guid.Empty));

        Assert.Equal("customerId", exception.ParamName);
    }

    [Fact]
    public void AddLine_WithMatchingOrderAndCurrency_AddsLineAndUpdatesTotal()
    {
        var order = SalesOrder.Create("ORDER-001", Guid.NewGuid(), "USD");
        var line = CreateLine(order, quantity: 3, unitPrice: 12.50m);

        order.AddLine(line);

        Assert.Single(order.Lines);
        Assert.Same(line, order.Lines.Single());
        Assert.Equal(37.50m, order.TotalAmount);
    }

    [Fact]
    public void AddLine_ForDifferentOrder_ThrowsInvalidOperationException()
    {
        var order = SalesOrder.Create("ORDER-001", Guid.NewGuid());
        var differentOrder = SalesOrder.Create("ORDER-002", Guid.NewGuid());
        var line = CreateLine(differentOrder, quantity: 1, unitPrice: 10);

        var exception = Assert.Throws<InvalidOperationException>(() => order.AddLine(line));

        Assert.Equal("The order line belongs to a different order.", exception.Message);
    }

    [Fact]
    public void AddLine_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        var order = SalesOrder.Create("ORDER-001", Guid.NewGuid(), "USD");
        var line = CreateLine(order, quantity: 1, unitPrice: 10, currency: "EUR");

        var exception = Assert.Throws<InvalidOperationException>(() => order.AddLine(line));

        Assert.Equal("The order line currency must match the order currency.", exception.Message);
    }

    [Fact]
    public void Confirm_WithLines_ChangesStatusToConfirmed()
    {
        var order = SalesOrder.Create("ORDER-001", Guid.NewGuid());
        order.AddLine(CreateLine(order, quantity: 1, unitPrice: 10));

        order.Confirm();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_WithoutLines_ThrowsInvalidOperationException()
    {
        var order = SalesOrder.Create("ORDER-001", Guid.NewGuid());

        var exception = Assert.Throws<InvalidOperationException>(() => order.Confirm());

        Assert.Equal("An order must contain at least one line.", exception.Message);
    }

    [Fact]
    public void Cancel_DraftOrder_ChangesStatusToCancelled()
    {
        var order = SalesOrder.Create("ORDER-001", Guid.NewGuid());

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_CancelledOrder_ThrowsInvalidOperationException()
    {
        var order = SalesOrder.Create("ORDER-001", Guid.NewGuid());
        order.Cancel();

        var exception = Assert.Throws<InvalidOperationException>(() => order.Cancel());

        Assert.Equal("This order cannot be cancelled.", exception.Message);
    }

    [Fact]
    public void UpdateShipmentStatus_WhenSomeQuantityIsShipped_SetsPartiallyShipped()
    {
        var order = SalesOrder.Create("ORDER-001", Guid.NewGuid());
        var line = CreateLine(order, quantity: 4, unitPrice: 10);
        order.AddLine(line);
        line.Ship(2);

        order.UpdateShipmentStatus();

        Assert.Equal(OrderStatus.PartiallyShipped, order.Status);
    }

    [Fact]
    public void UpdateShipmentStatus_WhenAllLinesAreFullyShipped_SetsShipped()
    {
        var order = SalesOrder.Create("ORDER-001", Guid.NewGuid());
        var line = CreateLine(order, quantity: 2, unitPrice: 10);
        order.AddLine(line);
        line.Ship(2);

        order.UpdateShipmentStatus();

        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    private static SalesOrderLine CreateLine(
        SalesOrder order,
        decimal quantity,
        decimal unitPrice,
        string currency = "USD")
    {
        return SalesOrderLine.Create(
            order.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            quantity,
            unitPrice,
            currency);
    }
}
