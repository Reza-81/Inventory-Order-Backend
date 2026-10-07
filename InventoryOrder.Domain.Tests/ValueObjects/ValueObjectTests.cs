using InventoryOrder.Domain.ValueObjects;
using Xunit;

namespace InventoryOrder.Domain.Tests.ValueObjects;

public class ValueObjectTests
{
    [Fact]
    public void Sku_Create_NormalizesValueToUppercase()
    {
        var sku = Sku.Create(" coffee-001 ");

        Assert.Equal("COFFEE-001", sku.Value);
        Assert.Equal("COFFEE-001", sku.ToString());
    }

    [Fact]
    public void Money_Create_NormalizesCurrencyToUppercase()
    {
        var money = Money.Create(12.50m, "usd");

        Assert.Equal(12.50m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Money_Create_WithNegativeAmount_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Money.Create(-1));

        Assert.Equal("amount", exception.ParamName);
    }
}
