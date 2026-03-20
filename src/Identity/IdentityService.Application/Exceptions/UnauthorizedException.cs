namespace IdentityService.Application.Exceptions;

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message) : base(message, 401) { }
}