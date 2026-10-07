using InventoryOrder.Domain.ValueObjects;

namespace InventoryOrder.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public Address BillingAddress { get; private set; } = null!;
    public Address? ShippingAddress { get; private set; }
    public bool IsActive { get; private set; }

    public Customer() { }

    private Customer(string name, string email, string? phone, Address billingAddress, Address? shippingAddress, bool isActive)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        BillingAddress = billingAddress;
        ShippingAddress = shippingAddress;
        IsActive = isActive;
    }

    public static Customer Create(string name, string email, Address billingAddress, Address? shippingAddress = null, string? phone = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        ArgumentNullException.ThrowIfNull(billingAddress);

        return new Customer(name.Trim(), email.Trim(), phone?.Trim(), billingAddress, shippingAddress, isActive);
    }
}
