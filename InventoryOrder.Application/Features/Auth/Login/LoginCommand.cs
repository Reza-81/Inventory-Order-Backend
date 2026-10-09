using InventoryOrder.Application.Common.Messaging;

namespace InventoryOrder.Application.Features.Auth.Login;

public sealed record LoginCommand(string Username, string Password)
    : ICommand<LoginResponse>;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string Username,
    string Role);
