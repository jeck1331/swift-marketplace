using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CatalogService.Infrastructure.Repositories;

public class StockRepository: IStockRepository
{
     private readonly string _connectionString;

    public StockRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")!;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<StockItem?> GetByProductIdAsync(
        Guid productId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, product_id, quantity, reserved, updated_at
            FROM stock
            WHERE product_id = @ProductId
            """;

        await using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<StockItem>(
            new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: ct));
    }

    public async Task SetStockAsync(
        Guid productId, int quantity, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO stock (id, product_id, quantity, reserved, updated_at)
            VALUES (@Id, @ProductId, @Quantity, 0, now())
            ON CONFLICT (product_id)
            DO UPDATE SET quantity = @Quantity, updated_at = now()
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = quantity
            }, cancellationToken: ct));
    }

    /// <summary>
    /// Атомарное резервирование через WHERE условие.
    /// Если остатков не хватает — UPDATE затронет 0 строк.
    /// Это решает проблему race condition без explicit lock.
    /// </summary>
    public async Task<bool> ReserveAsync(
        Guid productId, int quantity, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE stock
            SET reserved = reserved + @Quantity,
                updated_at = now()
            WHERE product_id = @ProductId
              AND (quantity - reserved) >= @Quantity
            """;

        await using var connection = CreateConnection();
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                ProductId = productId,
                Quantity = quantity
            }, cancellationToken: ct));

        return affected > 0;
    }

    /// <summary>
    /// Compensating transaction — отмена резерва.
    /// </summary>
    public async Task ReleaseReserveAsync(
        Guid productId, int quantity, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE stock
            SET reserved = reserved - @Quantity,
                updated_at = now()
            WHERE product_id = @ProductId
              AND reserved >= @Quantity
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                ProductId = productId,
                Quantity = quantity
            }, cancellationToken: ct));
    }

    /// <summary>
    /// После успешной оплаты — списываем из quantity и reserved.
    /// </summary>
    public async Task ConfirmReserveAsync(
        Guid productId, int quantity, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE stock
            SET quantity = quantity - @Quantity,
                reserved = reserved - @Quantity,
                updated_at = now()
            WHERE product_id = @ProductId
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                ProductId = productId,
                Quantity = quantity
            }, cancellationToken: ct));
    }
}