namespace InventoryOrder.Domain.ValueObjects;

public sealed record Sku
{
    public string Value { get; private set; } = null!;

    private Sku() { }

    private Sku(string value) => Value = value;

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU is required.", nameof(value));

        return new Sku(value.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;
}
