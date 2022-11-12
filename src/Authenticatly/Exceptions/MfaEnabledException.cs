namespace Authenticatly.Exceptions;

internal class MfaEnabledException : AuthenticatlyAuthException
{
    public string MfaToken { get; set; }

    public MfaEnabledException(string mfaToken)
    {
        MfaToken = mfaToken;
    }
}
