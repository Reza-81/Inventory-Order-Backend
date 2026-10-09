namespace InventoryOrder.Application.Common.Interfaces;

public interface ITransactionManager
{
    Task<TResponse> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> operation,
        int maxConcurrencyRetries = 0,
        CancellationToken cancellationToken = default);
}
