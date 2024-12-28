namespace Authenticatly.Requests;

internal class ChallengeRequest
{
    public required string MfaToken { get; set; }
    public required string ChallengeType { get; set; }
}
