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

    void Add<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
