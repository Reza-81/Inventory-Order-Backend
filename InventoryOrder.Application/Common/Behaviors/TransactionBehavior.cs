using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Application.Common.Messaging;
using MediatR;

namespace InventoryOrder.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(
    ITransactionManager transactionManager)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return transactionManager.ExecuteAsync(
            _ => next(cancellationToken),
            request is IConcurrencyRetryCommand<TResponse> retryCommand
                ? retryCommand.MaxConcurrencyRetries
                : 0,
            cancellationToken);
    }
}
