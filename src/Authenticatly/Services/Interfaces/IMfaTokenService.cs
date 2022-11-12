namespace Authenticatly.Services.Interfaces;

public interface IMfaTokenService
{
    public string? GetUserIdFromMfaToken(string token);
    public Task RemoveTokenAsync(string token);
    public Task<string> GenerateMfaTokenAsync(string userId, string provider, string name);
}
