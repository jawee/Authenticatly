using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Authenticatly.Authorization;
using Authenticatly.Services;
using Authenticatly.Exceptions;
using Authenticatly.HttpResults;
using Authenticatly.Responses;
using Authenticatly.Requests;
using Authenticatly.Services.Interfaces;
using Authenticatly.Utils;

namespace Authenticatly;

public class AuthenticatlyAuthOptions : IOptions<AuthenticatlyAuthOptions>
{
    public int TokenValidTimeInMinutes { get; set; } = 10;
    public string Issuer { get; set; } = "Authenticatly";
    public string Audience { get; set; }
    public string SymmetricSecurityKey { get; set; }
    public string AllowedRolesString { get; set; }

    public List<string> AllowedRoles
    {
        get
        {
            if (string.IsNullOrEmpty(AllowedRolesString))
            {
                return new();
            }
            return AllowedRolesString.Split(";").ToList();
        }
    }

    public AuthenticatlyAuthOptions Value => this;
}
public static class RegisterAuthenticatlyAuthenticationEndpointsBuilderExtensions
{
    public static IServiceCollection AddAuthenticatlyAuthentication<TUser>(this IServiceCollection services, Action<AuthenticatlyAuthOptions> setupAction) where TUser : IdentityUser
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        services.Configure(setupAction);

        services.AddTransient<ITokenService, TokenService>();

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(AuthenticatlyPolicyNames.AuthenticatlyPolicy).AddRequirements(new AuthenticatlyAuthorizeRequirement()).Build();
        });

        services.AddSingleton<IAuthorizationPolicyProvider, AuthenticatlyPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, AuthenticatlyAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthenticatlyAuthorizationMiddlewareResultHandler>();
        services.AddTransient<ILoginService, LoginService>();
        services.AddTransient<JwtSecurityTokenHandler>();

        return services;
    }

    public static IApplicationBuilder AddAuthenticatlyMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<AuthenticatlyClaimsMiddleware>();
        return app;
    }

    public static IdentityBuilder AddAuthenticatlyTokenProviders<TUser>(this IdentityBuilder builder, string issuer) where TUser : IdentityUser
    {
        return builder.AddTokenProvider(issuer, typeof(AuthenticatlyRefreshTokenProvider<TUser>));
    }


    public static TBuilder RequireAuthenticatlyAuth<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.RequireAuthorization(new AuthenticatlyAuthorizeAttribute());
    }

    public static void AddAuthenticatlyAuthEndpointsV1<TUser>(this IEndpointRouteBuilder app) where TUser : IdentityUser, new()
    {
        app.MapPost("/auth/v1/login", LoginV1<TUser>).AllowAnonymous().Produces<TokenResponse>(200).Produces(401).Produces(400).Produces<LoginChallengeResponse>(403);
        app.MapPost("/auth/v1/challenge", ChallengeV1<TUser>).AllowAnonymous().Produces<ChallengeResponse>(200);
        app.MapPost("/auth/v1/logout", LogoutV1<TUser>).RequireAuthenticatlyAuth();
    }

    private static async Task<string> LogoutV1<TUser>(HttpContext context, UserManager<TUser> userManager, IOptions<AuthenticatlyAuthOptions> optionsAccessor) where TUser : IdentityUser
    {
        var options = optionsAccessor.Value;

        var emailClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        if (emailClaim == null)
        {
            return "not logged in";
        }
        var user = await userManager.FindByEmailAsync(emailClaim.Value);
        await userManager.RemoveAuthenticationTokenAsync(user, options.Issuer, "RefreshToken");
        return "logged out";
    }

    private static async Task<IResult> ChallengeV1<TUser>([FromBody] ChallengeRequest request, UserManager<TUser> userManager, IMfaTokenService mfaTokenService, ISendSmsService sendSmsService, ILoginService loginService) where TUser : IdentityUser
    {
        try
        {
            var challengeResp = await loginService.Challenge(request, userManager, mfaTokenService, sendSmsService);
            return Results.Ok(challengeResp);
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ForbiddenException e)
        {
            return Results.Extensions.ForbidResult(e.ErrorMessage);
        }
        catch (AuthenticatlyAuthException)
        {
            return Results.BadRequest();
        }
    }


    private static async Task<IResult> LoginV1<TUser>([FromBody] AuthenticateRequest request, HttpContext httpContext, UserManager<TUser> userManager, ITokenService tokenService, IOptions<AuthenticatlyAuthOptions> optionsAccessor, ILoginService loginService) where TUser : IdentityUser, new()
    {
        try
        {
            var result = await loginService.Login(request, userManager, tokenService, optionsAccessor);
            return Results.Ok(result);
        }
        catch (MfaEnabledException e)
        {
            var errResp = new LoginChallengeResponse { Error = "mfa_required", MfaToken = e.MfaToken };
            return Results.Extensions.ForbidResult(errResp);
        }
        catch (OtpInvalidException)
        {
            //response.StatusCode = StatusCodes.Status401Unauthorized;
            return Results.Unauthorized();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (AuthenticatlyAuthException)
        {
            //response.StatusCode = StatusCodes.Status400BadRequest;
            return Results.BadRequest(AuthenticatlyAuthConstants.ErrorMessages.LOGIN_FAILED);
        }
    }
}
