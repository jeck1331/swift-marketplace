using IdentityService.Domain.Entities;

namespace IdentityService.Application;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct = default);
    Task<Guid?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task BlacklistAccessTokenAsync(string jti, TimeSpan ttl, CancellationToken ct = default);
    Task<bool> IsAccessTokenBlacklistedAsync(string jti, CancellationToken ct = default);
}