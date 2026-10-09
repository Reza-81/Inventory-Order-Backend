using InventoryOrder.Application.Common.Messaging;

namespace InventoryOrder.Application.Features.Inventory.GetStockLevels;

public sealed record GetStockLevelsQuery(Guid WarehouseId)
    : ICacheableQuery<IReadOnlyCollection<StockLevelResponse>>
{
    public string CacheKey => $"stock:warehouse:{WarehouseId}:all";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}

public sealed record StockLevelResponse(
    Guid ProductId,
    string Sku,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable);
