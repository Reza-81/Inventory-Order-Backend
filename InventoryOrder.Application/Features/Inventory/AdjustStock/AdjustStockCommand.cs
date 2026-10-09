using InventoryOrder.Application.Common.Messaging;

namespace InventoryOrder.Application.Features.Inventory.AdjustStock;

public sealed record AdjustStockCommand(
    Guid WarehouseId,
    Guid ProductId,
    decimal Delta,
    string Reason,
    string IdempotencyKey)
    : IIdempotentCommand<AdjustStockResponse>, IConcurrencyRetryCommand<AdjustStockResponse>
{
    public int MaxConcurrencyRetries => 3;
}

public sealed record AdjustStockResponse(
    Guid ProductId,
    Guid WarehouseId,
    decimal QuantityOnHand,
    decimal QuantityReserved);
