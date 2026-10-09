namespace InventoryOrder.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public string EntityName { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string? OldValuesJson { get; private set; }
    public string? NewValuesJson { get; private set; }
    public Guid? ActorId { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }

    public AuditLog() { }

    private AuditLog(string entityName, string entityId, string action, string? oldValuesJson, string? newValuesJson, Guid? actorId)
    {
        Id = Guid.NewGuid();
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        OldValuesJson = oldValuesJson;
        NewValuesJson = newValuesJson;
        ActorId = actorId;
        Timestamp = DateTimeOffset.UtcNow;
    }

    public static AuditLog Create(string entityName, string entityId, string action, string? oldValuesJson, string? newValuesJson, Guid? actorId)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Entity name is required.", nameof(entityName));
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID is required.", nameof(entityId));
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Audit action is required.", nameof(action));

        return new AuditLog(entityName.Trim(), entityId.Trim(), action.Trim().ToUpperInvariant(), oldValuesJson, newValuesJson, actorId);
    }
}
