using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.ValueObjects;
using Xunit;

namespace InventoryOrder.Domain.Tests.Entities;

public class WarehouseTests
{
    private static Address ValidAddress() =>
        Address.Create("1 Main Street", "Tehran", "1234567890", "Iran");

    [Fact]
    public void Create_WithValidValues_CreatesWarehouse()
    {
        var warehouse = Warehouse.Create(" wh-01 ", " Main Warehouse ", ValidAddress());

        Assert.NotEqual(Guid.Empty, warehouse.Id);
        Assert.Equal("WH-01", warehouse.Code);
        Assert.Equal("Main Warehouse", warehouse.Name);
        Assert.True(warehouse.IsActive);
    }

    [Fact]
    public void Create_WithEmptyCode_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Warehouse.Create("", "Main Warehouse", ValidAddress()));

        Assert.Equal("code", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Warehouse.Create("WH-01", "", ValidAddress()));

        Assert.Equal("name", exception.ParamName);
    }
}
