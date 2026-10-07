namespace InventoryOrder.Domain.Enums;

public enum OrderStatus
{
    Draft = 1,
    Confirmed = 2,
    PartiallyShipped = 3,
    Shipped = 4,
    Cancelled = 5,
}
