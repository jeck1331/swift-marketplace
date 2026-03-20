namespace IdentityService.Application.Exceptions;

public abstract class ApplicationException : Exception
{
    public int StatusCode { get; }
    
    protected ApplicationException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}