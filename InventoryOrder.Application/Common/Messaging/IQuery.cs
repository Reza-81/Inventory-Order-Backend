using MediatR;

namespace InventoryOrder.Application.Common.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>;

public interface ICacheableQuery<out TResponse> : IQuery<TResponse>
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}
