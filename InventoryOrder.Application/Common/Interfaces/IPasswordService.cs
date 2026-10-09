using InventoryOrder.Domain.Entities;

namespace InventoryOrder.Application.Common.Interfaces;

public interface IPasswordService
{
    string Hash(ApplicationUser user, string password);
    bool Verify(ApplicationUser user, string password);
}
