using InventoryOrder.Domain.Entities;

namespace InventoryOrder.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Product> Products { get; }
    IQueryable<Warehouse> Warehouses { get; }
    IQueryable<StockItem> StockItems { get; }
    IQueryable<Customer> Customers { get; }
    IQueryable<SalesOrder> SalesOrders { get; }
    IQueryable<SalesOrderLine> SalesOrderLines { get; }
    IQueryable<Shipment> Shipments { get; }
    IQueryable<ShipmentLine> ShipmentLines { get; }
    IQueryable<InventoryTransaction> InventoryTransactions { get; }
    IQueryable<ApplicationUser> Users { get; }
    IQueryable<UserWarehouse> UserWarehouses { get; }
    IQueryable<AuditLog> AuditLogs { get; }

    Task<Product?> FindProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Product?> FindProductBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Warehouse?> FindWarehouseByIdAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<Customer?> FindCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<StockItem?> FindStockItemAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<SalesOrder?> FindSalesOrderWithLinesAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IdempotencyRecord?> FindIdempotencyRecordAsync(string key, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> FindUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> UserHasWarehouseAccessAsync(Guid userId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<StockLevel>> GetStockLevelsByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default);

    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record StockLevel(
    Guid ProductId,
    string Sku,
    decimal QuantityOnHand,
    decimal QuantityReserved);
