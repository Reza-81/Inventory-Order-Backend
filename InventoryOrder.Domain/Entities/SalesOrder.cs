using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.Common;

namespace InventoryOrder.Domain.Entities;

public class SalesOrder : IAuditable
{
    private readonly List<SalesOrderLine> _lines = [];

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public SalesOrder() { }

    private SalesOrder(string orderNumber, Guid customerId, string currency)
    {
        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        OrderDate = DateTimeOffset.UtcNow;
        Currency = currency;
        CreatedAt = OrderDate;
    }

    public static SalesOrder Create(string orderNumber, Guid customerId, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number is required.", nameof(orderNumber));
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        return new SalesOrder(orderNumber.Trim(), customerId, currency.Trim().ToUpperInvariant());
    }

    public void AddLine(SalesOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        EnsureStatus(OrderStatus.Draft);
        if (line.SalesOrderId != Id)
            throw new InvalidOperationException("The order line belongs to a different order.");
        if (!string.Equals(line.Currency, Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The order line currency must match the order currency.");
        _lines.Add(line);
        TotalAmount += line.LineTotal;
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

    public void Confirm()
    {
        EnsureStatus(OrderStatus.Draft);
        if (_lines.Count == 0)
            throw new InvalidOperationException("An order must contain at least one line.");
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Cancelled)
            throw new InvalidOperationException("This order cannot be cancelled.");
        Status = OrderStatus.Cancelled;
    }

    public void UpdateShipmentStatus()
    {
        if (_lines.Count == 0) return;
        if (_lines.All(line => line.IsFullyShipped)) Status = OrderStatus.Shipped;
        else if (_lines.Any(line => line.QuantityShipped > 0)) Status = OrderStatus.PartiallyShipped;
    }

    private void EnsureStatus(OrderStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Order must be {expected}.");
    }
}
