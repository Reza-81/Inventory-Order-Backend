namespace InventoryOrder.Domain.Common;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    Guid? CreatedBy { get; }
    DateTimeOffset? UpdatedAt { get; }
    Guid? UpdatedBy { get; }

    void SetCreatedAudit(DateTimeOffset timestamp, Guid? actorId);
    void SetUpdatedAudit(DateTimeOffset timestamp, Guid? actorId);
}
