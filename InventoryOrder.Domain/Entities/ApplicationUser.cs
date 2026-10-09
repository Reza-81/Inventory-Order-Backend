using InventoryOrder.Domain.Enums;

namespace InventoryOrder.Domain.Entities;

public sealed class ApplicationUser
{
    private readonly List<UserWarehouse> _warehouseAssignments = [];

    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<UserWarehouse> WarehouseAssignments => _warehouseAssignments.AsReadOnly();

    public ApplicationUser() { }

    private ApplicationUser(string username, string displayName, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Username = username;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public static ApplicationUser Create(
        string username,
        string displayName,
        string passwordHash,
        UserRole role,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        if (!Enum.IsDefined(role))
            throw new ArgumentException("User role is invalid.", nameof(role));

        return new ApplicationUser(
            username.Trim().ToLowerInvariant(),
            displayName.Trim(),
            passwordHash,
            role)
        {
            IsActive = isActive
        };
    }

    public void AssignToWarehouse(Guid warehouseId)
    {
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID is required.", nameof(warehouseId));
        if (_warehouseAssignments.Any(assignment => assignment.WarehouseId == warehouseId))
            return;

        _warehouseAssignments.Add(UserWarehouse.Create(Id, warehouseId));
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        PasswordHash = passwordHash;
    }
}
