using System.Text.Json;
using InventoryOrder.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace InventoryOrder.Infrastructure.Caching;

public sealed class RedisCacheService(IDistributedCache distributedCache) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await distributedCache.GetAsync(key, cancellationToken);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan duration, CancellationToken cancellationToken = default) =>
        distributedCache.SetAsync(
            key,
            JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = duration },
            cancellationToken);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        distributedCache.RemoveAsync(key, cancellationToken);
}
