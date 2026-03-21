using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CatalogService.Infrastructure.Repositories;

public class ProductRepository: IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")!;
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT p.id, p.name, p.description, p.price,
                   p.category_id, p.image_url, p.is_active,
                   p.created_at, p.updated_at,
                   c.name AS category_name
            FROM products p
            INNER JOIN categories c ON c.id = p.category_id
            WHERE p.id = @Id
            """;

        await using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        const string sql = """
            SELECT p.id, p.name, p.description, p.price,
                   p.category_id, p.image_url, p.is_active,
                   p.created_at, p.updated_at,
                   c.name AS category_name
            FROM products p
            INNER JOIN categories c ON c.id = p.category_id
            WHERE p.category_id = @CategoryId AND p.is_active = true
            ORDER BY p.created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<Product>(
            new CommandDefinition(sql, new
            {
                CategoryId = categoryId,
                PageSize = pageSize,
                Offset = (page - 1) * pageSize
            }, cancellationToken: ct));

        return result.AsList();
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        const string sql = """
            SELECT p.id, p.name, p.description, p.price,
                   p.category_id, p.image_url, p.is_active,
                   p.created_at, p.updated_at,
                   c.name AS category_name
            FROM products p
            INNER JOIN categories c ON c.id = p.category_id
            WHERE p.is_active = true
            ORDER BY p.created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<Product>(
            new CommandDefinition(sql, new
            {
                PageSize = pageSize,
                Offset = (page - 1) * pageSize
            }, cancellationToken: ct));

        return result.AsList();
    }

    public async Task<Guid> CreateAsync(Product product, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO products (id, name, description, price, category_id, image_url, is_active, created_at)
            VALUES (@Id, @Name, @Description, @Price, @CategoryId, @ImageUrl, @IsActive, @CreatedAt)
            RETURNING id
            """;

        await using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, product, cancellationToken: ct));
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE products
            SET name = @Name,
                description = @Description,
                price = @Price,
                category_id = @CategoryId,
                image_url = @ImageUrl,
                updated_at = @UpdatedAt
            WHERE id = @Id
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, product, cancellationToken: ct));
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE products
            SET is_active = false, updated_at = now()
            WHERE id = @Id
            """;

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<int> GetTotalCountAsync(
        Guid? categoryId = null, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM products
            WHERE is_active = true
              AND (@CategoryId IS NULL OR category_id = @CategoryId)
            """;

        await using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { CategoryId = categoryId }, cancellationToken: ct));
    }
}