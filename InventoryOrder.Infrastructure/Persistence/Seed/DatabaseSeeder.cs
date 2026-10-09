using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.ValueObjects;
using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryOrder.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext dbContext,
        IPasswordService passwordService,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await dbContext.Warehouses.FirstOrDefaultAsync(cancellationToken);
        if (warehouse is null)
        {
            warehouse = Warehouse.Create(
                "MAIN",
                "Main warehouse",
                Address.Create("1 Main Street", "Tehran", "11111", "Iran"));
            dbContext.Add(warehouse);
        }

        if (!await dbContext.Products.AnyAsync(cancellationToken))
        {
            var product = Product.Create(
                Sku.Create("DEMO-001"),
                "Demo product",
                UnitOfMeasure.Kg,
                Money.Create(10m),
                reorderLevel: 5m);
            var customer = Customer.Create(
                "Demo customer",
                "demo@example.com",
                Address.Create("2 Customer Street", "Tehran", "22222", "Iran"));
            var stockItem = StockItem.Create(product.Id, warehouse.Id);
            stockItem.Receive(100m);
            dbContext.AddRange(product, customer, stockItem);
        }

        if (!await dbContext.Users.AnyAsync(cancellationToken))
        {
            var admin = ApplicationUser.Create("admin", "System administrator", "seed", UserRole.Admin);
            admin.SetPasswordHash(passwordService.Hash(admin, "Admin123!"));
            var manager = ApplicationUser.Create("manager", "Warehouse manager", "seed", UserRole.WarehouseManager);
            manager.SetPasswordHash(passwordService.Hash(manager, "Manager123!"));
            manager.AssignToWarehouse(warehouse.Id);
            dbContext.AddRange(admin, manager);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
