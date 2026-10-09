using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.ValueObjects;
using InventoryOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryOrder.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken))
            return;

        var product = Product.Create(
            Sku.Create("DEMO-001"),
            "Demo product",
            UnitOfMeasure.Kg,
            Money.Create(10m),
            reorderLevel: 5m);

        var warehouse = Warehouse.Create(
            "MAIN",
            "Main warehouse",
            Address.Create("1 Main Street", "Tehran", "11111", "Iran"));

        var customer = Customer.Create(
            "Demo customer",
            "demo@example.com",
            Address.Create("2 Customer Street", "Tehran", "22222", "Iran"));

        var stockItem = StockItem.Create(product.Id, warehouse.Id);
        stockItem.Receive(100m);

        dbContext.AddRange(product, warehouse, customer, stockItem);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
