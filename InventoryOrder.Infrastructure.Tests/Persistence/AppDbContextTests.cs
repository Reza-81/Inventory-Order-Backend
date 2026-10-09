using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.ValueObjects;
using InventoryOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryOrder.Infrastructure.Tests.Persistence;

public sealed class AppDbContextTests
{
    [Fact]
    public async Task Product_CanBeSavedAndLoadedWithOwnedValueObjects()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var writeContext = CreateContext(databaseName);

        var product = Product.Create(
            Sku.Create(" coffee-001 "),
            "Coffee",
            UnitOfMeasure.Box,
            Money.Create(12.50m, "usd"),
            reorderLevel: 10);

        writeContext.Products.Add(product);
        await writeContext.SaveChangesAsync();

        await using var readContext = CreateContext(databaseName);
        var savedProduct = await readContext.Products
            .SingleAsync(x => x.Id == product.Id);

        Assert.Equal("COFFEE-001", savedProduct.Sku.Value);
        Assert.Equal("Coffee", savedProduct.Name);
        Assert.Equal(12.50m, savedProduct.Price.Amount);
        Assert.Equal("USD", savedProduct.Price.Currency);
    }

    [Fact]
    public async Task Customer_CanBeSavedAndLoadedWithOptionalShippingAddress()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = CreateContext(databaseName);

        var customer = Customer.Create(
            "Customer",
            "customer@example.com",
            Address.Create("Billing street", "Tehran", "11111", "Iran"));

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var savedCustomer = await context.Customers.SingleAsync();

        Assert.Equal("Billing street", savedCustomer.BillingAddress.Line1);
        Assert.Null(savedCustomer.ShippingAddress);
    }

    [Fact]
    public async Task StockItem_ConcurrentUpdateThrowsConcurrencyException()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var setupContext = CreateContext(databaseName))
        {
            var product = Product.Create(
                Sku.Create("STOCK-001"),
                "Stock product",
                UnitOfMeasure.Box,
                Money.Create(10),
                reorderLevel: 1);
            var warehouse = Warehouse.Create(
                "MAIN",
                "Main warehouse",
                Address.Create("Warehouse street", "Tehran", "22222", "Iran"));
            var stockItem = StockItem.Create(product.Id, warehouse.Id);
            stockItem.Receive(10);

            setupContext.AddRange(product, warehouse, stockItem);
            await setupContext.SaveChangesAsync();
        }

        await using var firstContext = CreateContext(databaseName);
        await using var secondContext = CreateContext(databaseName);
        var firstStockItem = await firstContext.StockItems.SingleAsync();
        var secondStockItem = await secondContext.StockItems.SingleAsync();

        firstStockItem.Adjust(1);
        await firstContext.SaveChangesAsync();

        secondStockItem.Adjust(1);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => secondContext.SaveChangesAsync());
    }

    [Fact]
    public void Model_ContainsAllPersistenceEntities()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        Assert.NotNull(context.Model.FindEntityType(typeof(Product)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Warehouse)));
        Assert.NotNull(context.Model.FindEntityType(typeof(StockItem)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Customer)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SalesOrder)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SalesOrderLine)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Shipment)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ShipmentLine)));
        Assert.NotNull(context.Model.FindEntityType(typeof(InventoryTransaction)));
    }

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }
}
