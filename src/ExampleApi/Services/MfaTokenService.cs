using Authenticatly.Services.Interfaces;
using ExampleApi.Context;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace ExampleApi.Services;

public class MfaTokenService : IMfaTokenService
{

    private readonly MyDbContext _context;

	public MfaTokenService(MyDbContext context)
	{
		_context = context;
	}

	public async Task<string> GenerateMfaTokenAsync(string userId, string provider, string name)
	{
		var value = GenerateToken();
		var userToken = new IdentityUserToken<string>
		{
			LoginProvider = provider,
			Name = name,
			UserId = userId,
			Value = value
		};

		await _context.UserTokens.AddAsync(userToken);
		await _context.SaveChangesAsync();

		return value;
	}
    private string GenerateToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }


    public string? GetUserIdFromMfaToken(string token)
	{
		var userId = _context.UserTokens.Where(t => t.Value.Equals(token)).Select(t => t.UserId).FirstOrDefault();
		return userId;
	}

	public async Task RemoveTokenAsync(string token)
	{
		var tok = _context.UserTokens.Where(t => t.Value.Equals(token)).FirstOrDefault();
		if(tok is null)
		{
			return;
		}
		_context.UserTokens.Remove(tok);
		await _context.SaveChangesAsync();
	}
}
