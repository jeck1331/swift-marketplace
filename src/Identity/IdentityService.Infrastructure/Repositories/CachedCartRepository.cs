using System.Text.Json;
using IdentityService.Application;
using IdentityService.Domain.Entities;
using StackExchange.Redis;

namespace IdentityService.Infrastructure.Repositories;

public class CachedCartRepository : ICartRepository
{
    private readonly CartRepository _postgres;
    private readonly IDatabase _redis;
    private const string KeyPrefix = "cart:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public CachedCartRepository(CartRepository postgres, IConnectionMultiplexer redis)
    {
        _postgres = postgres;
        _redis = redis.GetDatabase();
    }

    public async Task<IReadOnlyList<CartItem>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        var cacheKey = $"{KeyPrefix}{userId}";

        // 1. Попробовать из Redis
        var cached = await _redis.StringGetAsync(cacheKey);
        if (!cached.IsNullOrEmpty)
            return JsonSerializer.Deserialize<List<CartItem>>(cached!.ToString())!;

        // 2. Из Postgres
        var items = await _postgres.GetByUserIdAsync(userId, ct);

        // 3. Положить в Redis
        await _redis.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(items),
            CacheTtl);

        return items;
    }

    public async Task UpsertAsync(CartItem item, CancellationToken ct = default)
    {
        await _postgres.UpsertAsync(item, ct);
        await InvalidateCacheAsync(item.UserId);
    }

    public async Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        await _postgres.RemoveAsync(userId, productId, ct);
        await InvalidateCacheAsync(userId);
    }

    public async Task ClearAsync(Guid userId, CancellationToken ct = default)
    {
        await _postgres.ClearAsync(userId, ct);
        await InvalidateCacheAsync(userId);
    }

    private async Task InvalidateCacheAsync(Guid userId)
    {
        await _redis.KeyDeleteAsync($"{KeyPrefix}{userId}");
    }
}