using InventoryOrder.Application.Common.Messaging;

namespace InventoryOrder.Application.Features.Ping;

public sealed record PingQuery(string Message) : IQuery<PingResponse>;

public sealed record PingResponse(string Message);
