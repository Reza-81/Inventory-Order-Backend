using InventoryOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace InventoryOrder.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
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
