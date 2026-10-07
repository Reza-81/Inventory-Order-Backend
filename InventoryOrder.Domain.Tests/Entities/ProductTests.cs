using InventoryOrder.Domain.Entities;
using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.ValueObjects;
using Xunit;

namespace InventoryOrder.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidValues_CreatesProduct()
    {
        var product = Product.Create(
            Sku.Create(" coffee-001 "),
            " Coffee ",
            UnitOfMeasure.Box,
            Money.Create(12.50m),
            reorderLevel: 10);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("COFFEE-001", product.Sku.Value);
        Assert.Equal("Coffee", product.Name);
        Assert.Equal(UnitOfMeasure.Box, product.UnitOfMeasure);
        Assert.Equal(12.50m, product.Price.Amount);
        Assert.Equal(10, product.ReorderLevel);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Create_WithNegativeReorderLevel_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create(
                Sku.Create("coffee-001"),
                "Coffee",
                UnitOfMeasure.Box,
                Money.Create(12.50m),
                reorderLevel: -1));

        Assert.Equal("reorderLevel", exception.ParamName);
    }

    [Fact]
    public void Create_WithInvalidUnitOfMeasure_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create(
                Sku.Create("coffee-001"),
                "Coffee",
                (UnitOfMeasure)999,
                Money.Create(12.50m),
                reorderLevel: 0));

        Assert.Equal("unitOfMeasure", exception.ParamName);
    }
}
