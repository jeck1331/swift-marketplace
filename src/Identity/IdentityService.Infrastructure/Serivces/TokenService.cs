using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Application;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace IdentityService.Infrastructure.Serivces;

public class TokenService : ITokenService
{
     private readonly IDatabase _redis;
    private readonly JwtSettings _jwtSettings;

    public TokenService(
        IConnectionMultiplexer redis,
        IOptions<JwtSettings> jwtSettings)
    {
        _redis = redis.GetDatabase();
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenLifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task SaveRefreshTokenAsync(
        Guid userId, string refreshToken, CancellationToken ct = default)
    {
        var key = $"refresh:{refreshToken}";
        var ttl = TimeSpan.FromDays(_jwtSettings.RefreshTokenLifetimeDays);

        await _redis.StringSetAsync(key, userId.ToString(), ttl);
    }

    public async Task<Guid?> ValidateRefreshTokenAsync(
        string refreshToken, CancellationToken ct = default)
    {
        var value = await _redis.StringGetAsync($"refresh:{refreshToken}");

        if (value.IsNullOrEmpty)
            return null;

        return Guid.Parse(value!.ToString());
    }

    public async Task RevokeRefreshTokenAsync(
        string refreshToken, CancellationToken ct = default)
    {
        await _redis.KeyDeleteAsync($"refresh:{refreshToken}");
    }

    public async Task BlacklistAccessTokenAsync(
        string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        // Храним до момента истечения access token
        await _redis.StringSetAsync($"blacklist:{jti}", "revoked", ttl);
    }

    public async Task<bool> IsAccessTokenBlacklistedAsync(
        string jti, CancellationToken ct = default)
    {
        return await _redis.KeyExistsAsync($"blacklist:{jti}");
    }
}