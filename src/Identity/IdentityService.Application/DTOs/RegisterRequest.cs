namespace IdentityService.Application.DTO;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string? Phone);