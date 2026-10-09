using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InventoryOrder.Infrastructure.Security;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public Guid? UserId => Guid.TryParse(
        Principal.FindFirstValue(ClaimTypes.NameIdentifier),
        out var userId) ? userId : null;

    public string? Username => Principal.FindFirstValue(ClaimTypes.Name);
    public string? Role => Principal.FindFirstValue(ClaimTypes.Role);
    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;
    public bool IsInRole(string role) => Principal.IsInRole(role);
}

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<ApplicationUser> _hasher = new();

    public string Hash(ApplicationUser user, string password) => _hasher.HashPassword(user, password);

    public bool Verify(ApplicationUser user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password) is PasswordVerificationResult.Success;
}

public sealed class JwtAccessTokenService(IConfiguration configuration) : IAccessTokenService
{
    public AccessToken Create(ApplicationUser user)
    {
        var secret = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is missing from configuration.");
        var issuer = configuration["Jwt:Issuer"] ?? "InventoryOrder.Api";
        var audience = configuration["Jwt:Audience"] ?? "InventoryOrder.Client";
        var lifetimeMinutes = configuration.GetValue("Jwt:LifetimeMinutes", 60);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(lifetimeMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

public sealed class WarehouseAccessChecker(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser) : IWarehouseAccessChecker
{
    public Task<bool> CanAccessAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        if (currentUser.IsInRole("Admin"))
            return Task.FromResult(true);
        if (currentUser.UserId is not Guid userId)
            return Task.FromResult(false);

        return dbContext.UserHasWarehouseAccessAsync(userId, warehouseId, cancellationToken);
    }
}
