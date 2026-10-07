using InventoryOrder.Domain.Enums;
using InventoryOrder.Domain.ValueObjects;

namespace InventoryOrder.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public UnitOfMeasure UnitOfMeasure { get; private set; }
    public Money Price { get; private set; } = null!;
    public decimal ReorderLevel { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

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
}
