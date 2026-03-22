using Dapper;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository: IOrderRepository
{
     private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")!;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<Domain.Entities.Order?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, user_id, status, total_amount,
                   created_at, updated_at, paid_at,
                   cancelled_at, cancel_reason
            FROM orders
            WHERE id = @Id
            """;

        await using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Domain.Entities.Order>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Domain.Entities.Order>> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, user_id, status, total_amount,
                   created_at, updated_at, paid_at,
                   cancelled_at, cancel_reason
            FROM orders
            WHERE user_id = @UserId
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<Domain.Entities.Order>(
            new CommandDefinition(sql, new
            {
                UserId = userId,
                PageSize = pageSize,
                Offset = (page - 1) * pageSize
            }, cancellationToken: ct));

        return result.AsList();
    }

    public async Task<Guid> CreateAsync(
        Domain.Entities.Order order, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO orders (id, user_id, status, total_amount, created_at)
            VALUES (@Id, @UserId, @Status, @TotalAmount, @CreatedAt)
            RETURNING id
            """;

        await using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, new
            {
                order.Id,
                order.UserId,
                Status = (int)order.Status,
                order.TotalAmount,
                order.CreatedAt
            }, cancellationToken: ct));
    }

    public async Task UpdateStatusAsync(
        Guid id, OrderStatus status, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE orders
            SET status = @Status, updated_at = now()
            WHERE id = @Id
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                Id = id,
                Status = (int)status
            }, cancellationToken: ct));
    }

    public async Task SetPaidAsync(
        Guid id, DateTime paidAt, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE orders
            SET status = @Status, paid_at = @PaidAt, updated_at = now()
            WHERE id = @Id
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                Id = id,
                Status = (int)OrderStatus.Paid,
                PaidAt = paidAt
            }, cancellationToken: ct));
    }

    public async Task SetCancelledAsync(
        Guid id, string reason, DateTime cancelledAt, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE orders
            SET status = @Status, cancel_reason = @Reason,
                cancelled_at = @CancelledAt, updated_at = now()
            WHERE id = @Id
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                Id = id,
                Status = (int)OrderStatus.Cancelled,
                Reason = reason,
                CancelledAt = cancelledAt
            }, cancellationToken: ct));
    }
}