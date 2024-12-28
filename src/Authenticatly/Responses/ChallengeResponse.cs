namespace Authenticatly.Responses;
internal class ChallengeResponse
{
    public required string ChallengeType { get; set; }
    public required string BindingMethod { get; set; }
    public string? OobCode { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; internal set; } = new();
}
