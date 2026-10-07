using System;

namespace InventoryOrder.Domain.Entities;

public class Warehouse
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public bool IsActive { get; private set; }

    /// <summary>
    /// For EF Core
    /// </summary>
    public Warehouse() {}

    private Warehouse(string code, string name, string address, bool isActive)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Address = address;
        IsActive = isActive;
    }

    /// <summary>
    /// Factory Method
    /// </summary>
    /// <param name="code"></param>
    /// <param name="name"></param>
    /// <param name="address"></param>
    /// <param name="isActive"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Warehouse Create(string code, string name, string address, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required.", nameof(address));

        return new Warehouse(code, name, address, isActive);
    }
}
