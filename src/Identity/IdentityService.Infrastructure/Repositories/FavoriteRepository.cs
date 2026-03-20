using Dapper;
using IdentityService.Application;
using IdentityService.Application.Exceptions;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IdentityService.Infrastructure.Repositories;

public class FavoriteRepository: IFavoriteRepository
{
     private readonly string _connectionString;

    public FavoriteRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new ArgumentNullException("Postgres connection string is missing");
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IReadOnlyList<FavoriteItem>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, user_id, product_id, added_at
            FROM favorites
            WHERE user_id = @UserId
            ORDER BY added_at DESC
            """;

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<FavoriteItem>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));

        return result.AsList();
    }

    public async Task AddAsync(FavoriteItem item, CancellationToken ct = default)
    {
        // ON CONFLICT DO NOTHING — добавление в избранное идемпотентно
        // повторный клик "в избранное" не должен падать с ошибкой
        const string sql = """
            INSERT INTO favorites (id, user_id, product_id, added_at)
            VALUES (@Id, @UserId, @ProductId, @AddedAt)
            ON CONFLICT (user_id, product_id) DO NOTHING
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, item, cancellationToken: ct));
    }

    public async Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        const string sql = """
            DELETE FROM favorites
            WHERE user_id = @UserId AND product_id = @ProductId
            """;

        await using var connection = CreateConnection();
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, ProductId = productId },
                cancellationToken: ct));

        if (affected == 0)
            throw new NotFoundException(
                $"Favorite with product {productId} not found for user {userId}");
    }
}