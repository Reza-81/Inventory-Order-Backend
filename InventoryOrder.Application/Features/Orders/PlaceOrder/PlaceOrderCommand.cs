using InventoryOrder.Application.Common.Messaging;

namespace InventoryOrder.Application.Features.Orders.PlaceOrder;

public sealed record OrderLineDto(string Sku, Guid WarehouseId, decimal Quantity);

public sealed record PlaceOrderCommand(
    Guid CustomerId,
    IReadOnlyCollection<OrderLineDto> Lines,
    string IdempotencyKey)
    : IIdempotentCommand<PlaceOrderResponse>;

public sealed record PlaceOrderResponse(
    Guid OrderId,
    string OrderNumber,
    decimal TotalAmount,
    string Currency,
    string Status);
