using Microsoft.AspNetCore.Identity;

namespace Authenticatly.Authorization
{
    public class AuthenticatlyTotpProvider<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : IdentityUser
    {
        // Should never be called
        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(false);
        }

        // Should never be called
        public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            throw new NotImplementedException();
        }

        // Validate TOTP sent from client
        public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            throw new NotImplementedException();
        }
    }
}
