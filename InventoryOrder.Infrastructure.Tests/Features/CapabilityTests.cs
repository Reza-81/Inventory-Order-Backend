using InventoryOrder.Application.Common.Behaviors;
using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Application.Features.Auth.Login;
using InventoryOrder.Application.Features.Inventory.GetStockLevels;
using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.ValueObjects;
using InventoryOrder.Infrastructure.Persistence;
using InventoryOrder.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryOrder.Infrastructure.Tests.Features;

public sealed class CapabilityTests
{
    [Fact]
    public async Task SaveChanges_CreatesAuditEntryWithActorAndNewValues()
    {
        var actorId = Guid.NewGuid();
        await using var context = CreateContext(new TestCurrentUser(actorId, "Admin"));
        var product = Product.Create(
            Sku.Create("AUDIT-001"),
            "Audit product",
            UnitOfMeasure.Box,
            Money.Create(10),
            reorderLevel: 1);
        context.Products.Add(product);

        await context.SaveChangesAsync();
        var audit = await context.AuditLogs.SingleAsync(log => log.EntityId == product.Id.ToString());

        Assert.Equal(actorId, audit.ActorId);
        Assert.Equal("ADDED", audit.Action);
        Assert.Contains("Audit product", audit.NewValuesJson);
    }

    [Fact]
    public async Task LoginHandler_ReturnsTokenForValidCredentials()
    {
        await using var context = CreateContext();
        var passwordService = new PasswordService();
        var user = ApplicationUser.Create("sales", "Sales representative", "seed", UserRole.SalesRep);
        user.SetPasswordHash(passwordService.Hash(user, "Sales123!"));
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new LoginHandler(context, passwordService, new TestTokenService());
        var response = await handler.Handle(new LoginCommand("SALES", "Sales123!"), CancellationToken.None);

        Assert.Equal(user.Id, response.UserId);
        Assert.Equal("sales", response.Username);
        Assert.Equal("SalesRep", response.Role);
        Assert.Equal("test-token", response.AccessToken);
    }

    [Fact]
    public async Task WarehouseManager_CanAdjustOnlyAssignedWarehouse()
    {
        await using var context = CreateContext();
        var warehouse = Warehouse.Create(
            "MAIN",
            "Main warehouse",
            Address.Create("Warehouse street", "Tehran", "11111", "Iran"));
        var product = Product.Create(
            Sku.Create("SCOPE-001"),
            "Scoped product",
            UnitOfMeasure.Box,
            Money.Create(10),
            reorderLevel: 1);
        var user = ApplicationUser.Create("manager", "Warehouse manager", "seed", UserRole.WarehouseManager);
        user.AssignToWarehouse(warehouse.Id);
        context.AddRange(warehouse, product, user, StockItem.Create(product.Id, warehouse.Id));
        await context.SaveChangesAsync();

        var currentUser = new TestCurrentUser(user.Id, "WarehouseManager");
        var checker = new WarehouseAccessChecker(context, currentUser);
        var response = await new InventoryOrder.Application.Features.Inventory.AdjustStock.AdjustStockHandler(
            context,
            currentUser,
            checker).Handle(
                new InventoryOrder.Application.Features.Inventory.AdjustStock.AdjustStockCommand(
                    warehouse.Id,
                    product.Id,
                    4,
                    "Receipt",
                    "scope-key"),
                CancellationToken.None);

        Assert.Equal(4, response.QuantityOnHand);
    }

    [Fact]
    public async Task CacheBehavior_ReusesCachedResponse()
    {
        var cache = new MemoryCacheService();
        var behavior = new CacheBehavior<GetStockLevelsQuery, IReadOnlyCollection<StockLevelResponse>>(cache);
        var query = new GetStockLevelsQuery(Guid.NewGuid());
        var expected = new[] { new StockLevelResponse(Guid.NewGuid(), "CACHE-001", 8, 2, 6) };
        var calls = 0;

        var first = await behavior.Handle(query, _ =>
        {
            calls++;
            return Task.FromResult<IReadOnlyCollection<StockLevelResponse>>(expected);
        }, CancellationToken.None);
        var second = await behavior.Handle(query, _ =>
        {
            calls++;
            return Task.FromResult<IReadOnlyCollection<StockLevelResponse>>(Array.Empty<StockLevelResponse>());
        }, CancellationToken.None);

        Assert.Equal(expected, first);
        Assert.Equal(expected, second);
        Assert.Equal(1, calls);
    }

    private static AppDbContext CreateContext(ICurrentUser? currentUser = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options, currentUser);
    }

    private sealed class TestCurrentUser(Guid userId, string role) : ICurrentUser
    {
        public Guid? UserId => userId;
        public string? Username => "test-user";
        public string? Role => role;
        public bool IsAuthenticated => true;
        public bool IsInRole(string requestedRole) => string.Equals(role, requestedRole, StringComparison.Ordinal);
    }

    private sealed class TestTokenService : IAccessTokenService
    {
        public AccessToken Create(ApplicationUser user) =>
            new("test-token", DateTimeOffset.UtcNow.AddMinutes(1));
    }

    private sealed class MemoryCacheService : ICacheService
    {
        private readonly Dictionary<string, object> _values = [];

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(_values.TryGetValue(key, out var value) && value is T typed ? (T?)typed : default);

        public Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            _values[key] = value!;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _values.Remove(key);
            return Task.CompletedTask;
        }
    }
}
