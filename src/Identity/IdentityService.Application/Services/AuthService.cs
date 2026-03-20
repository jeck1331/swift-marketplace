using IdentityService.Application.DTO;
using IdentityService.Application.Exceptions;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepo;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepo, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var existing = await _userRepo.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new ConflictException($"User with email {request.Email} already exists");
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.CreateAsync(user, ct);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken, ct);

        return new AuthResponse(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(15));
    }
    
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.ToLowerInvariant(), ct)
                   ?? throw new UnauthorizedException("Invalid credentials");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken, ct);

        return new AuthResponse(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(15));
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        var userId = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken, ct)
                     ?? throw new UnauthorizedException("Invalid refresh token");

        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, ct);

        var user = await _userRepo.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException("User not found");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken, ct);

        return new AuthResponse(accessToken, newRefreshToken, DateTime.UtcNow.AddMinutes(15));
    }

    public async Task LogoutAsync(string jti, string refreshToken, CancellationToken ct)
    {
        // Blacklist current access token
        await _tokenService.BlacklistAccessTokenAsync(jti, TimeSpan.FromMinutes(15), ct);
        // Revoke refresh token
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, ct);
    }
}