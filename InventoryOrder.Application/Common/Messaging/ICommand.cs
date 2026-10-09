using MediatR;

namespace InventoryOrder.Application.Common.Messaging;

public interface ICommand<out TResponse> : IRequest<TResponse>;

public interface IConcurrencyRetryCommand<out TResponse> : ICommand<TResponse>
{
    int MaxConcurrencyRetries { get; }
}

public interface IIdempotentCommand<out TResponse> : ICommand<TResponse>
{
    string IdempotencyKey { get; }
}
