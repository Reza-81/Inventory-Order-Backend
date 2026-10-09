using MediatR;

namespace InventoryOrder.Application.Common.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>;
