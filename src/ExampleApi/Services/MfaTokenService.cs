using Authenticatly.Services.Interfaces;
using ExampleApi.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    public async Task<string?> GetUserIdFromMfaToken(string token)
    {
        var userId = await _context.UserTokens.Where(t => t.Value!.Equals(token)).Select(t => t.UserId).FirstOrDefaultAsync();
        return userId;
    }

    public async Task RemoveTokenAsync(string token)
    {
        var tok = await _context.UserTokens.Where(t => t.Value!.Equals(token)).FirstOrDefaultAsync();
        if (tok is null)
        {
            return;
        }
        _context.UserTokens.Remove(tok);
        await _context.SaveChangesAsync();
    }
}
