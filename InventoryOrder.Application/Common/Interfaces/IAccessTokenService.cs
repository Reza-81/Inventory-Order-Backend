using InventoryOrder.Domain.Entities;

namespace InventoryOrder.Application.Common.Interfaces;

public interface IAccessTokenService
{
    AccessToken Create(ApplicationUser user);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
