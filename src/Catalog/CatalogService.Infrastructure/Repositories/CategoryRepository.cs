using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CatalogService.Infrastructure.Repositories;

public class CategoryRepository: ICategoryRepository
{
    private readonly string _connectionString;

    public CategoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")!;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, name, description, parent_id, created_at
            FROM categories
            ORDER BY name
            """;

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<Category>(
            new CommandDefinition(sql, cancellationToken: ct));
        return result.AsList();
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, name, description, parent_id, created_at
            FROM categories
            WHERE id = @Id
            """;

        await using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Category>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<Guid> CreateAsync(Category category, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO categories (id, name, description, parent_id, created_at)
            VALUES (@Id, @Name, @Description, @ParentId, @CreatedAt)
            RETURNING id
            """;

        await using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, category, cancellationToken: ct));
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE categories
            SET name = @Name, description = @Description
            WHERE id = @Id
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, category, cancellationToken: ct));
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM categories WHERE id = @Id";

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
}