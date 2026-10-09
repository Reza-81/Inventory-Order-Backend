using InventoryOrder.Domain.Entities;
using InventoryOrder.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace InventoryOrder.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();
    public DbSet<InventoryTransaction> InventoryTransactions =>
        Set<InventoryTransaction>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<UserWarehouse> UserWarehouses => Set<UserWarehouse>();

    IQueryable<Product> IApplicationDbContext.Products => Products;
    IQueryable<Warehouse> IApplicationDbContext.Warehouses => Warehouses;
    IQueryable<StockItem> IApplicationDbContext.StockItems => StockItems;
    IQueryable<Customer> IApplicationDbContext.Customers => Customers;
    IQueryable<SalesOrder> IApplicationDbContext.SalesOrders => SalesOrders;
    IQueryable<SalesOrderLine> IApplicationDbContext.SalesOrderLines => SalesOrderLines;
    IQueryable<Shipment> IApplicationDbContext.Shipments => Shipments;
    IQueryable<ShipmentLine> IApplicationDbContext.ShipmentLines => ShipmentLines;
    IQueryable<InventoryTransaction> IApplicationDbContext.InventoryTransactions => InventoryTransactions;
    IQueryable<ApplicationUser> IApplicationDbContext.Users => Users;
    IQueryable<UserWarehouse> IApplicationDbContext.UserWarehouses => UserWarehouses;
    Task<Product?> IApplicationDbContext.FindProductByIdAsync(Guid productId, CancellationToken cancellationToken) =>
        Products.SingleOrDefaultAsync(product => product.Id == productId, cancellationToken);
    Task<Product?> IApplicationDbContext.FindProductBySkuAsync(string sku, CancellationToken cancellationToken) =>
        Products.SingleOrDefaultAsync(product => product.Sku.Value == sku, cancellationToken);
    Task<Warehouse?> IApplicationDbContext.FindWarehouseByIdAsync(Guid warehouseId, CancellationToken cancellationToken) =>
        Warehouses.SingleOrDefaultAsync(warehouse => warehouse.Id == warehouseId, cancellationToken);
    Task<Customer?> IApplicationDbContext.FindCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken) =>
        Customers.SingleOrDefaultAsync(customer => customer.Id == customerId, cancellationToken);
    Task<StockItem?> IApplicationDbContext.FindStockItemAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken) =>
        StockItems.SingleOrDefaultAsync(stock => stock.ProductId == productId && stock.WarehouseId == warehouseId, cancellationToken);
    Task<SalesOrder?> IApplicationDbContext.FindSalesOrderWithLinesAsync(Guid orderId, CancellationToken cancellationToken) =>
        SalesOrders.Include(order => order.Lines).SingleOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    Task<IdempotencyRecord?> IApplicationDbContext.FindIdempotencyRecordAsync(string key, CancellationToken cancellationToken) =>
        IdempotencyRecords.SingleOrDefaultAsync(record => record.Key == key, cancellationToken);
    Task<ApplicationUser?> IApplicationDbContext.FindUserByUsernameAsync(string username, CancellationToken cancellationToken) =>
        Users.SingleOrDefaultAsync(user => user.Username == username, cancellationToken);
    Task<bool> IApplicationDbContext.UserHasWarehouseAccessAsync(Guid userId, Guid warehouseId, CancellationToken cancellationToken) =>
        UserWarehouses.AnyAsync(assignment => assignment.UserId == userId && assignment.WarehouseId == warehouseId, cancellationToken);
    void IApplicationDbContext.Add<TEntity>(TEntity entity) => Add(entity);
    void IApplicationDbContext.Remove<TEntity>(TEntity entity) => Remove(entity);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetStockConcurrencyTokens();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        SetStockConcurrencyTokens();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetStockConcurrencyTokens()
    {
        foreach (var entry in ChangeTracker.Entries<StockItem>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var token = new byte[16];
            RandomNumberGenerator.Fill(token);
            entry.Property(nameof(StockItem.RowVersion)).CurrentValue = token;
        }
    }
}
