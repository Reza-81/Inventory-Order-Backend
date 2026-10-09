namespace InventoryOrder.Domain.Entities;

public sealed class IdempotencyRecord
{
    public string Key { get; private set; } = null!;
    public string RequestHash { get; private set; } = null!;
    public string ResponseJson { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    public IdempotencyRecord() { }

    private IdempotencyRecord(
        string key,
        string requestHash,
        string responseJson,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        Key = key;
        RequestHash = requestHash;
        ResponseJson = responseJson;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public static IdempotencyRecord Create(
        string key,
        string requestHash,
        string responseJson,
        TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key is required.", nameof(key));
        if (string.IsNullOrWhiteSpace(requestHash))
            throw new ArgumentException("Request hash is required.", nameof(requestHash));
        if (string.IsNullOrWhiteSpace(responseJson))
            throw new ArgumentException("Response JSON is required.", nameof(responseJson));
        if (lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime));

        var now = DateTimeOffset.UtcNow;
        return new IdempotencyRecord(key.Trim(), requestHash, responseJson, now, now.Add(lifetime));
    }

    public bool IsExpired(DateTimeOffset now) => ExpiresAt <= now;
}
