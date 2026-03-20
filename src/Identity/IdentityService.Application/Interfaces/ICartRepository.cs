using IdentityService.Domain.Entities;

namespace IdentityService.Application;

public interface ICartRepository
{
    Task<IReadOnlyList<CartItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task UpsertAsync(CartItem item, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task ClearAsync(Guid userId, CancellationToken ct = default);
}