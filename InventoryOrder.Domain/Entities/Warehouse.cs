using InventoryOrder.Domain.ValueObjects;

namespace InventoryOrder.Domain.Entities;

public class Warehouse
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public Warehouse() { }

    private Warehouse(string code, string name, Address address, bool isActive)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Address = address;
        IsActive = isActive;
    }

    public static Warehouse Create(string code, string name, Address address, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        ArgumentNullException.ThrowIfNull(address);

        return new Warehouse(code.Trim().ToUpperInvariant(), name.Trim(), address, isActive);
    }
}
