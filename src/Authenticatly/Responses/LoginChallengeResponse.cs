namespace Authenticatly.Responses;

internal class LoginChallengeResponse
{
    public string Error { get; set; }
    public string MfaToken { get; set; }
}
