using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Application.Common.Messaging;
using InventoryOrder.Domain.Entities;
using MediatR;

namespace InventoryOrder.Application.Common.Behaviors;

public sealed class IdempotencyBehavior<TRequest, TResponse>(
    IApplicationDbContext dbContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IIdempotentCommand<TResponse>
{
    private static readonly TimeSpan RecordLifetime = TimeSpan.FromHours(24);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestHash = ComputeHash(request);
        var idempotencyKey = request.IdempotencyKey.Trim();
        var existingRecord = await dbContext.FindIdempotencyRecordAsync(
            idempotencyKey,
            cancellationToken);

        if (existingRecord is not null && !existingRecord.IsExpired(DateTimeOffset.UtcNow))
        {
            if (!string.Equals(existingRecord.RequestHash, requestHash, StringComparison.Ordinal))
                throw new ConflictException(
                    "The idempotency key has already been used with a different request.");

            return DeserializeResponse(existingRecord.ResponseJson);
        }

        if (existingRecord is not null)
            dbContext.Remove(existingRecord);

        var response = await next(cancellationToken);
        var responseJson = JsonSerializer.Serialize(response);

        dbContext.Add(IdempotencyRecord.Create(
            idempotencyKey,
            requestHash,
            responseJson,
            RecordLifetime));

        return response;
    }

    private static string ComputeHash(TRequest request)
    {
        var requestJson = JsonSerializer.Serialize(request);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(requestJson));
        return Convert.ToHexString(hash);
    }

    private static TResponse DeserializeResponse(string responseJson)
    {
        var response = JsonSerializer.Deserialize<TResponse>(responseJson);
        return response
            ?? throw new InvalidOperationException("The stored idempotent response could not be deserialized.");
    }
}
