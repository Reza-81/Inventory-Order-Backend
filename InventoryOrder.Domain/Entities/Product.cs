using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.ValueObjects;
using InventoryOrder.Domain.Common;

namespace InventoryOrder.Domain.Entities;

public class Product : IAuditable
{
    public Guid Id { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public UnitOfMeasure UnitOfMeasure { get; private set; }
    public Money Price { get; private set; } = null!;
    public decimal ReorderLevel { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public Product() { }

    private Product(Sku sku, string name, UnitOfMeasure unitOfMeasure, Money price, decimal reorderLevel, bool isActive)
    {
        Id = Guid.NewGuid();
        Sku = sku;
        Name = name;
        UnitOfMeasure = unitOfMeasure;
        Price = price;
        ReorderLevel = reorderLevel;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
        CreatedAt = UpdatedAt.Value;
    }

    public static Product Create(Sku sku, string name, UnitOfMeasure unitOfMeasure, Money price, decimal reorderLevel, bool isActive = true)
    {
        ArgumentNullException.ThrowIfNull(sku);
        ArgumentNullException.ThrowIfNull(price);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (!Enum.IsDefined(unitOfMeasure))
            throw new ArgumentException("The unit of measure is invalid.", nameof(unitOfMeasure));
        if (reorderLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative.", nameof(reorderLevel));

        return new Product(sku, name.Trim(), unitOfMeasure, price, reorderLevel, isActive);
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
