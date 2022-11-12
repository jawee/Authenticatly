namespace Authenticatly.Requests;

internal class ChallengeRequest
{
    public string MfaToken { get; set; }
    public string ChallengeType { get; set; }
}
