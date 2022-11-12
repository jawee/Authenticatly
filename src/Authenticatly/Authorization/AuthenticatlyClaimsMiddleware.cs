using Authenticatly.Services;
using Authenticatly.Utils;
using Microsoft.AspNetCore.Http;

namespace Authenticatly.Authorization;

internal class AuthenticatlyClaimsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenService _tokenService;
    public AuthenticatlyClaimsMiddleware(RequestDelegate next, ITokenService tokenService)
    {
        _next = next;
        _tokenService = tokenService;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.ToString();

            var claimsPrincipal = _tokenService.GetClaimsPrincipalFromToken(token);
            httpContext.User = claimsPrincipal;

            var additionalProperties = claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);

            httpContext.Items.Add(AuthenticatlyAuthConstants.AUTHORIZED_ATTRIBUTES_KEY, additionalProperties);
        }
        await _next(httpContext);
    }
}
