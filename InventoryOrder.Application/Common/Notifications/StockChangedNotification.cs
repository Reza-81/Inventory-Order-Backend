using InventoryOrder.Application.Common.Interfaces;
using MediatR;

namespace InventoryOrder.Application.Common.Notifications;

public sealed record StockChangedNotification(Guid WarehouseId) : INotification;

public sealed class StockCacheInvalidationHandler(ICacheService cacheService)
    : INotificationHandler<StockChangedNotification>
{
    public Task Handle(StockChangedNotification notification, CancellationToken cancellationToken) =>
        cacheService.RemoveAsync(
            StockCacheKeys.ForWarehouse(notification.WarehouseId),
            cancellationToken);
}

public static class StockCacheKeys
{
    public static string ForWarehouse(Guid warehouseId) => $"stock:warehouse:{warehouseId}:all";
    public static string ForProduct(Guid warehouseId, Guid productId) => $"stock:{warehouseId}:{productId}";
}
