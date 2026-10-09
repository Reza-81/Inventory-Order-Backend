using InventoryOrder.Application.Common.Messaging;

namespace InventoryOrder.Application.Features.Orders.ShipOrder;

public sealed record ShipOrderLineDto(Guid SalesOrderLineId, decimal Quantity);

public sealed record ShipOrderCommand(
    Guid OrderId,
    string ShipmentNumber,
    IReadOnlyCollection<ShipOrderLineDto> Lines,
    string? Carrier,
    string? TrackingNumber,
    string IdempotencyKey)
    : IIdempotentCommand<ShipOrderResponse>, IConcurrencyRetryCommand<ShipOrderResponse>
{
    public int MaxConcurrencyRetries => 3;
}

public sealed record ShipOrderResponse(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid OrderId,
    string OrderStatus,
    string ShipmentStatus);
