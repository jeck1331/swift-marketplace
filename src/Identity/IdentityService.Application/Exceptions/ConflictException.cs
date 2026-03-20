namespace IdentityService.Application.Exceptions;

public class ConflictException : ApplicationException
{
    public ConflictException(string message) : base(message, 409) { }
}