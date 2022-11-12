namespace Authenticatly.Requests;
public record AuthenticateRequest(string? Email = null, string? Password = null, string? RefreshToken = null, string? MfaToken = null, string? challengeType = null, string? Otp = null, string? OobCode  = null);
