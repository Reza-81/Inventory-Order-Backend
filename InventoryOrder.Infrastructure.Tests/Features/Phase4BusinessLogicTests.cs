using InventoryOrder.Application.Common.Behaviors;
using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Features.Inventory.AdjustStock;
using InventoryOrder.Application.Features.Orders.PlaceOrder;
using InventoryOrder.Application.Features.Orders.ShipOrder;
using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.ValueObjects;
using InventoryOrder.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryOrder.Infrastructure.Tests.Features;

public sealed class Phase4BusinessLogicTests
{
    [Fact]
    public async Task PlaceOrder_ReservesStockAndWritesReserveTransaction()
    {
        await using var context = await CreateSeededContext();
        var handler = new PlaceOrderHandler(context);
        var command = await CreatePlaceOrderCommand(context);

        var response = await handler.Handle(command, CancellationToken.None);
        await context.SaveChangesAsync();

        var stockItem = await context.StockItems.SingleAsync();
        var order = await context.SalesOrders.Include(value => value.Lines).SingleAsync();

        Assert.Equal(OrderStatus.Confirmed.ToString(), response.Status);
        Assert.Equal(3m, stockItem.QuantityReserved);
        Assert.Equal(3m, order.Lines.Single().QuantityOrdered);
        Assert.Equal(TransactionType.Reserve, await context.InventoryTransactions.Select(value => value.Type).SingleAsync());
    }

    [Fact]
    public async Task ShipOrder_DecrementsStockAndMarksOrderShipped()
    {
        await using var context = await CreateSeededContext();
        var command = await CreatePlaceOrderCommand(context);
        var placeResponse = await new PlaceOrderHandler(context)
            .Handle(command, CancellationToken.None);
        await context.SaveChangesAsync();

        var orderLineId = await context.SalesOrderLines.Select(line => line.Id).SingleAsync();
        var response = await new ShipOrderHandler(context).Handle(
            new ShipOrderCommand(
                placeResponse.OrderId,
                "SHIP-001",
                [new ShipOrderLineDto(orderLineId, 3)],
                "UPS",
                "TRACK-001",
                "ship-key-001"),
            CancellationToken.None);
        await context.SaveChangesAsync();

        var stockItem = await context.StockItems.SingleAsync();
        var order = await context.SalesOrders.SingleAsync();

        Assert.Equal(OrderStatus.Shipped.ToString(), response.OrderStatus);
        Assert.Equal(7m, stockItem.QuantityOnHand);
        Assert.Equal(0m, stockItem.QuantityReserved);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal(TransactionType.Ship, await context.InventoryTransactions
            .Where(value => value.Type == TransactionType.Ship)
            .Select(value => value.Type)
            .SingleAsync());
    }

    [Fact]
    public async Task AdjustStock_UpdatesBalanceAndWritesAdjustmentTransaction()
    {
        await using var context = await CreateSeededContext();
        var productId = await context.Products.Select(product => product.Id).SingleAsync();
        var warehouseId = await context.Warehouses.Select(warehouse => warehouse.Id).SingleAsync();

        var response = await new AdjustStockHandler(context).Handle(
            new AdjustStockCommand(
                warehouseId,
                productId,
                -2,
                "Damaged stock",
                "adjust-key-001"),
            CancellationToken.None);
        await context.SaveChangesAsync();

        Assert.Equal(8m, response.QuantityOnHand);
        Assert.Equal(TransactionType.Adjust, await context.InventoryTransactions
            .Select(value => value.Type)
            .SingleAsync());
        Assert.Equal(-2m, await context.InventoryTransactions
            .Select(value => value.Quantity)
            .SingleAsync());
    }

    [Fact]
    public async Task IdempotencyBehavior_ReplaysSameResponseAndRejectsDifferentRequest()
    {
        await using var context = await CreateSeededContext();
        var behavior = new IdempotencyBehavior<PlaceOrderCommand, PlaceOrderResponse>(context);
        var command = await CreatePlaceOrderCommand(context);
        var expected = new PlaceOrderResponse(Guid.NewGuid(), "ORD-001", 30, "USD", "Confirmed");
        var handlerCalls = 0;

        var first = await behavior.Handle(
            command,
            _ =>
            {
                handlerCalls++;
                return Task.FromResult(expected);
            },
            CancellationToken.None);
        await context.SaveChangesAsync();

        var second = await behavior.Handle(
            command,
            _ => throw new InvalidOperationException("The handler should not run for a replay."),
            CancellationToken.None);

        var conflictingCommand = command with { Lines = [new OrderLineDto("COFFEE-001", command.Lines.Single().WarehouseId, 4)] };

        await Assert.ThrowsAsync<ConflictException>(() => behavior.Handle(
            conflictingCommand,
            _ => Task.FromResult(expected),
            CancellationToken.None));

        Assert.Equal(expected, first);
        Assert.Equal(expected, second);
        Assert.Equal(1, handlerCalls);
    }

    private static async Task<PlaceOrderCommand> CreatePlaceOrderCommand(AppDbContext context)
    {
        return new PlaceOrderCommand(
            await context.Customers.Select(customer => customer.Id).SingleAsync(),
            [new OrderLineDto("COFFEE-001", await context.Warehouses.Select(warehouse => warehouse.Id).SingleAsync(), 3)],
            "order-key-001");
    }

    private static async Task<AppDbContext> CreateSeededContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);

        var product = Product.Create(
            Sku.Create("COFFEE-001"),
            "Coffee",
            UnitOfMeasure.Box,
            Money.Create(10),
            reorderLevel: 1);
        var warehouse = Warehouse.Create(
            "MAIN",
            "Main warehouse",
            Address.Create("1 Main Street", "Tehran", "11111", "Iran"));
        var customer = Customer.Create(
            "Customer",
            "customer@example.com",
            Address.Create("2 Customer Street", "Tehran", "22222", "Iran"));
        var stockItem = StockItem.Create(product.Id, warehouse.Id);
        stockItem.Receive(10);

        context.AddRange(product, warehouse, customer, stockItem);
        await context.SaveChangesAsync();
        return context;
    }
}
