namespace InventoryOrder.Application.Common.Interfaces;

public interface IWarehouseAccessChecker
{
    Task<bool> CanAccessAsync(Guid warehouseId, CancellationToken cancellationToken = default);
}
