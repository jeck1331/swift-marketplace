using IdentityService.Domain.Entities;

namespace IdentityService.Application;

public interface IFavoriteRepository
{
    Task<IReadOnlyList<FavoriteItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(FavoriteItem item, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default);
}