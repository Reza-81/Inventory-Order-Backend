using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Common.Interfaces;
using MediatR;

namespace InventoryOrder.Application.Features.Auth.Login;

public sealed class LoginHandler(
    IApplicationDbContext dbContext,
    IPasswordService passwordService,
    IAccessTokenService accessTokenService)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.FindUserByUsernameAsync(
            request.Username.Trim().ToLowerInvariant(),
            cancellationToken);

        if (user is null || !user.IsActive || !passwordService.Verify(user, request.Password))
            throw new ConflictException("The username or password is incorrect.");

        var token = accessTokenService.Create(user);
        return new LoginResponse(token.Value, token.ExpiresAt, user.Id, user.Username, user.Role.ToString());
    }
}
