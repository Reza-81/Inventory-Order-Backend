using InventoryOrder.Domain.ValueObjects;
using InventoryOrder.Domain.Common;

namespace InventoryOrder.Domain.Entities;

public class Warehouse : IAuditable
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public Warehouse() { }

    private Warehouse(string code, string name, Address address, bool isActive)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Address = address;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
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

    public void SetCreatedAudit(DateTimeOffset timestamp, Guid? actorId)
    {
        CreatedAt = timestamp;
        CreatedBy = actorId;
        UpdatedAt = timestamp;
        UpdatedBy = actorId;
    }

    public void SetUpdatedAudit(DateTimeOffset timestamp, Guid? actorId)
    {
        UpdatedAt = timestamp;
        UpdatedBy = actorId;
    }
}
