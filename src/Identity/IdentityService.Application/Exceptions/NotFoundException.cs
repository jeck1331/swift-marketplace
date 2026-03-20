namespace IdentityService.Application.Exceptions;

public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message, 404) { }
}