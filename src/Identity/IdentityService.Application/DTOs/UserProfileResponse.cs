namespace IdentityService.Application.DTO;

public record UserProfileResponse(Guid Id, string Email, string FirstName, string LastName, string? Phone);