namespace IdentityService.Application.DTO;

public record UpdateProfileRequest(string FirstName, string LastName, string? Phone);