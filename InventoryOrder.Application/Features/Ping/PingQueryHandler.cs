using MediatR;

namespace InventoryOrder.Application.Features.Ping;

public sealed class PingQueryHandler : IRequestHandler<PingQuery, PingResponse>
{
    public Task<PingResponse> Handle(
        PingQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new PingResponse($"Pong: {request.Message}"));
    }
}
