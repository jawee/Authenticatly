using Authenticatly.Exceptions;
using Authenticatly.Requests;
using Authenticatly.Responses;
using Authenticatly.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Authenticatly.Services;

internal interface ILoginService
{
    Task<TokenResponse> Login<TUser>(AuthenticateRequest request, UserManager<TUser> userManager, ITokenService tokenService, IOptions<AuthenticatlyAuthOptions> optionsAccessor) where TUser : IdentityUser, new();
    Task<ChallengeResponse> Challenge<TUser>(ChallengeRequest request, UserManager<TUser> userManager, IMfaTokenService mfaTokenService, ISendSmsService sendSmsService) where TUser : IdentityUser;
}

internal class LoginService : ILoginService
{

    private readonly IMfaTokenService _mfaTokenService;
    private readonly IClaimsInjectionService _claimsInjectionService;

    public LoginService(IMfaTokenService mfaTokenService, IClaimsInjectionService claimsInjectionService)
    {
        _mfaTokenService = mfaTokenService;
        _claimsInjectionService = claimsInjectionService;
    }

    public async Task<ChallengeResponse> Challenge<TUser>(ChallengeRequest request, UserManager<TUser> userManager, IMfaTokenService mfaTokenService, ISendSmsService sendSmsService) where TUser : IdentityUser
    {
        var additionalParameters = new Dictionary<string, string>();
        var userId = await mfaTokenService.GetUserIdFromMfaToken(request.MfaToken);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException();
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            throw new UnauthorizedException();
        }

        if (request.ChallengeType.Equals("sms"))
        {
            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                throw new ForbiddenException("Could not send sms.");
            }

            var smsToken = await userManager.GenerateTwoFactorTokenAsync(user, "Phone");
            var isSent = await sendSmsService.SendSms(smsToken, user.PhoneNumber, userId);
            if (!isSent)
            {
                throw new ForbiddenException("Could not send sms.");
            }
            additionalParameters.Add("phonenumber", user.PhoneNumber[^4..]);

            var challengeResponse = new ChallengeResponse 
            {
                BindingMethod = "prompt",
                ChallengeType = "sms",
                OobCode = "",
                AdditionalProperties = additionalParameters,
            };
            return challengeResponse;
        }
        throw new UnsupportedChallengeTypeException();
    }

    public async Task<TokenResponse> Login<TUser>(AuthenticateRequest request, UserManager<TUser> userManager, ITokenService tokenService, IOptions<AuthenticatlyAuthOptions> optionsAccessor) where TUser : IdentityUser, new()
    {
        if (!string.IsNullOrEmpty(request.MfaToken))
        {
            var user = await FindUserByMfaToken(request.MfaToken, userManager);
            if (user is null)
            {
                throw new LoginFailedException();
            }

            ////MFA sms stuff
            var valid = await userManager.VerifyTwoFactorTokenAsync(user, "Phone", request.Otp ?? "");
            if (!valid)
            {
                throw new OtpInvalidException();
            }

            var token = await CreateTokenResponse(userManager, tokenService, user, optionsAccessor);
            return token;
        }
        else if (!string.IsNullOrEmpty(request.Email))
        {
            var options = optionsAccessor.Value;

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                throw new LoginFailedException();
            }

            var roles = await userManager.GetRolesAsync(user);

            if (options.AllowedRoles.Count != 0)
            {
                if (!roles.Any(s => options.AllowedRoles.Contains(s)))
                {
                    throw new UnauthorizedException();
                }
            }

            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                if (await userManager.VerifyUserTokenAsync(user, optionsAccessor.Value.Issuer, "RefreshToken", request.RefreshToken))
                {
                    var tokenResp = await CreateTokenResponse(userManager, tokenService, user, optionsAccessor);
                    return tokenResp;
                }

                throw new UnauthorizedException();
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                throw new LoginFailedException();
            }

            var result = await userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
            {
                throw new LoginFailedException();
            }

            if (user.TwoFactorEnabled)
            {
                var mfaToken = await GenerateMfaToken(user, options.Issuer);
                throw new MfaEnabledException(mfaToken);
            }
            var token = await CreateTokenResponse(userManager, tokenService, user, optionsAccessor);
            return token;
        }

        throw new LoginFailedException();
    }

    private async Task<TUser?> FindUserByMfaToken<TUser>(string mfaToken, UserManager<TUser> userManager) where TUser : IdentityUser
    {
        var userId = await _mfaTokenService.GetUserIdFromMfaToken(mfaToken);
        await _mfaTokenService.RemoveTokenAsync(mfaToken);
        if (userId == null)
        {
            return null;
        }
        var user = await userManager.FindByIdAsync(userId);
        await _mfaTokenService.RemoveTokenAsync(mfaToken);
        return user;
    }

    private async Task<string> GenerateMfaToken<TUser>(TUser user, string provider) where TUser : IdentityUser
    {
        var token = await _mfaTokenService.GenerateMfaTokenAsync(user.Id, provider, "MfaToken");

        return token;
    }

    private async Task<TokenResponse> CreateTokenResponse<TUser>(UserManager<TUser> userManager, ITokenService tokenService, TUser user, IOptions<AuthenticatlyAuthOptions> optionsAccessor) where TUser : IdentityUser, new()
    {
        var options = optionsAccessor.Value;
        var claims = await userManager.GetClaimsAsync(user);

        // only want unique claims
        claims = claims.DistinctBy(a => a.Type).ToList();

        if (!claims.Any(x => x.Type == ClaimTypes.Email))
        {
            if (string.IsNullOrEmpty(user.Email))
            {
                throw new Exception("User needs to have email");
            }
            var claim = new Claim(ClaimTypes.Email, user.Email);
            var res = await userManager.AddClaimAsync(user, claim);
            claims.Add(claim);
        }

        var extraClaims = await _claimsInjectionService.GetExtraClaimsForUserId(user.Id);

        claims = claims.Concat(extraClaims).ToList();

        await userManager.RemoveAuthenticationTokenAsync(user, options.Issuer, "RefreshToken");
        var refreshToken = await userManager.GenerateUserTokenAsync(user, options.Issuer, "RefreshToken");
        var token = tokenService.CreateTokenForUser(user, claims);
        var resp = new TokenResponse(token, "Bearer", options.TokenValidTimeInMinutes * 60, Array.Empty<string>(), refreshToken);
        return resp;
    }
}
