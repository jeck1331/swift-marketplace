using Dapper;
using IdentityService.Application;
using IdentityService.Application.Exceptions;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IdentityService.Infrastructure.Repositories;

public class CartRepository: ICartRepository
{
    private readonly string _connectionString;

    public CartRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new ArgumentNullException("Postgres connection string is missing");
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IReadOnlyList<CartItem>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, user_id, product_id, quantity, added_at, updated_at
            FROM cart_items
            WHERE user_id = @UserId
            ORDER BY added_at DESC
            """;

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<CartItem>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));

        return result.AsList();
    }

    public async Task UpsertAsync(CartItem item, CancellationToken ct = default)
    {
        // INSERT если нет, UPDATE quantity если уже есть
        // ON CONFLICT — идемпотентная операция, важно для собеседования
        const string sql = """
            INSERT INTO cart_items (id, user_id, product_id, quantity, added_at)
            VALUES (@Id, @UserId, @ProductId, @Quantity, @AddedAt)
            ON CONFLICT (user_id, product_id)
            DO UPDATE SET 
                quantity = @Quantity,
                updated_at = now()
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, item, cancellationToken: ct));
    }

    public async Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        const string sql = """
            DELETE FROM cart_items
            WHERE user_id = @UserId AND product_id = @ProductId
            """;

        await using var connection = CreateConnection();
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, ProductId = productId },
                cancellationToken: ct));

        if (affected == 0)
            throw new NotFoundException(
                $"Cart item with product {productId} not found for user {userId}");
    }

    public async Task ClearAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            DELETE FROM cart_items
            WHERE user_id = @UserId
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
    }
}