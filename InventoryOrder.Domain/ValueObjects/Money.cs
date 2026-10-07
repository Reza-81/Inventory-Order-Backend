namespace InventoryOrder.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;

    private Money() { }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        return new Money(amount, currency.Trim().ToUpperInvariant());
    }
    

}
