using System;
using InventoryOrder.Domain.Enums;

namespace InventoryOrder.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public Guid Sku { get; private set; }
    public string Name { get; private set; } = null!;
    public UniteOfMeasure UniteOfMeasure { get; private set; }
    public decimal ReOrderLevel { get; private set;}
    public bool IsActive { get; private set; }
    public DateOnly UpdatedAt { get; private set;}

    /// <summary>
    /// For EF Core
    /// </summary>
    public Product() { }

    private Product(
        Guid sku, 
        string name, 
        UniteOfMeasure uniteOfMeasure, 
        decimal reOrderLevel, 
        bool isActive, 
        DateOnly updatedAt)
    {
        Sku = sku;
        Name = name;
        UniteOfMeasure = uniteOfMeasure;
        ReOrderLevel = reOrderLevel;
        IsActive = isActive;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Factory Method
    /// </summary>
    /// <param name="sku"></param>
    /// <param name="name"></param>
    /// <param name="uniteOfMeasure"></param>
    /// <param name="reOrderLevel"></param>
    /// <param name="isActive"></param>
    /// <param name="updatedAt"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Product Create(Guid sku, 
        string name, 
        UniteOfMeasure uniteOfMeasure, 
        decimal reOrderLevel, 
        bool isActive, 
        DateOnly updatedAt)
    {
        if (sku == Guid.Empty)
            throw new ArgumentException("Sku is requrired.");

        if (name == string.Empty)
            throw new ArgumentException("Name is requrired.");
        
        if (Enum.IsDefined(typeof(UniteOfMeasure), uniteOfMeasure))
            throw new ArgumentException("The value must be a valid unite.");
        
        if (reOrderLevel < 0)
            throw new ArgumentException("Reorder level must be None negative.");

        return new Product(
            sku, 
            name, 
            uniteOfMeasure, 
            reOrderLevel, 
            isActive, 
            updatedAt);
    }



}
