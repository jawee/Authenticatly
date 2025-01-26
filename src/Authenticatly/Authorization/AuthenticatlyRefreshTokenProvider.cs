using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Authenticatly.Authorization;

public class AuthenticatlyRefreshTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : IdentityUser
{

    private readonly IUserStore<TUser> _userStore;
    private readonly AuthenticatlyAuthOptions _options;

    public AuthenticatlyRefreshTokenProvider(IUserStore<TUser> userStore, IOptions<AuthenticatlyAuthOptions> optionsAccessor)
    {
        _userStore = userStore;
        _options = optionsAccessor.Value;
    }


    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(false);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        if (_userStore is not IUserAuthenticationTokenStore<TUser> store)
        {
            throw new InvalidOperationException();
        }

        var refreshToken = AuthenticatlyRefreshTokenProvider<TUser>.GenerateRefreshToken();
        await store.SetTokenAsync(user, _options.Issuer, purpose, refreshToken, CancellationToken.None);
        await manager.UpdateAsync(user);
        return refreshToken;
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
    {
        if (_userStore is not IUserAuthenticationTokenStore<TUser> store)
        {
            throw new InvalidOperationException();
        }

        var storedToken = await store.GetTokenAsync(user, _options.Issuer, purpose, CancellationToken.None);
        var result = token.Equals(storedToken);

        await store.RemoveTokenAsync(user, _options.Issuer, purpose, CancellationToken.None);

        return result;
    }
}
