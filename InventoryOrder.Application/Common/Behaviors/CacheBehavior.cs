using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Application.Common.Messaging;
using MediatR;

namespace InventoryOrder.Application.Common.Behaviors;

public sealed class CacheBehavior<TRequest, TResponse>(ICacheService cacheService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<TResponse>(request.CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var response = await next(cancellationToken);
        await cacheService.SetAsync(request.CacheKey, response, request.CacheDuration, cancellationToken);
        return response;
    }
}
