namespace Authenticatly.Exceptions;

internal class ForbiddenException : AuthenticatlyAuthException
{
    public string ErrorMessage { get; set; }

    public ForbiddenException(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
