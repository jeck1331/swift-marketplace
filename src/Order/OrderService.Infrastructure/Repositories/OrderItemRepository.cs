using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly string _connectionString;

    public OrderItemRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")!;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(
        Guid orderId, CancellationToken ct = default)
    {
        const string sql = """
                           SELECT id, order_id, product_id, product_name, price, quantity
                           FROM order_items
                           WHERE order_id = @OrderId
                           """;

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<OrderItem>(
            new CommandDefinition(sql, new { OrderId = orderId }, cancellationToken: ct));

        return result.AsList();
    }

    public async Task CreateManyAsync(
        IEnumerable<OrderItem> items, CancellationToken ct = default)
    {
        const string sql = """
                           INSERT INTO order_items (id, order_id, product_id, product_name, price, quantity)
                           VALUES (@Id, @OrderId, @ProductId, @ProductName, @Price, @Quantity)
                           """;

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        // Используем транзакцию для batch insert
        await using var transaction = await connection.BeginTransactionAsync(ct);
        try
        {
            foreach (var item in items)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(sql, item, transaction: transaction,
                        cancellationToken: ct));
            }
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}