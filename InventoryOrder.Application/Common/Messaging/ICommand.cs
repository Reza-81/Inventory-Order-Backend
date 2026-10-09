using MediatR;

namespace InventoryOrder.Application.Common.Messaging;

public interface ICommand<out TResponse> : IRequest<TResponse>;
