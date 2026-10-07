namespace InventoryOrder.Domain.ValueObjects;

public sealed record Address
{
    public string Line1 { get; private set; } = null!;
    public string? Line2 { get; private set; }
    public string City { get; private set; } = null!;
    public string? State { get; private set; }
    public string PostalCode { get; private set; } = null!;
    public string Country { get; private set; } = null!;

    private Address() { }

    private Address(string line1, string? line2, string city, string? state, string postalCode, string country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string line1, string city, string postalCode, string country, string? line2 = null, string? state = null)
    {
        if (string.IsNullOrWhiteSpace(line1)) throw new ArgumentException("Address line 1 is required.", nameof(line1));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentException("Postal code is required.", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country is required.", nameof(country));

        return new Address(line1.Trim(), line2?.Trim(), city.Trim(), state?.Trim(), postalCode.Trim(), country.Trim());
    }
}
