using Dapper;
using IdentityService.Application;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
                            ?? throw new ArgumentNullException("Postgres connection string is missing");
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
                           SELECT id, email, password_hash, first_name, last_name,
                                  phone, role, created_at, updated_at
                           FROM users
                           WHERE id = @Id
                           """;

        await using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        const string sql = """
                           SELECT id, email, password_hash, first_name, last_name,
                                  phone, role, created_at, updated_at
                           FROM users
                           WHERE email = @Email
                           """;

        await using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Email = email }, cancellationToken: ct));
    }

    public async Task<Guid> CreateAsync(User user, CancellationToken ct = default)
    {
        const string sql = """
                           INSERT INTO users (id, email, password_hash, first_name, last_name, phone, role, created_at)
                           VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, @Phone, @Role, @CreatedAt)
                           RETURNING id
                           """;

        await using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(sql, user, cancellationToken: ct));
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        const string sql = """
                           UPDATE users
                           SET first_name = @FirstName,
                               last_name = @LastName,
                               phone = @Phone,
                               updated_at = @UpdatedAt
                           WHERE id = @Id
                           """;

        await using var connection = CreateConnection();
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, user, cancellationToken: ct));

        if (affected == 0)
            throw new Application.Exceptions.NotFoundException($"User {user.Id} not found");
    }
}