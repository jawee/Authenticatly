namespace Authenticatly.Responses;
internal class ChallengeResponse
{
    public string ChallengeType { get; set; }
    public string BindingMethod { get; set; }
    public string OobCode { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; internal set; }
}
