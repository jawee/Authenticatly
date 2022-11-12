using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Authenticatly.Authorization;

public class AuthenticatlyAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _handler = new();

    public AuthenticatlyAuthorizationMiddlewareResultHandler()
    {
    }

    public async Task HandleAsync(RequestDelegate requestDelegate, HttpContext httpContext, AuthorizationPolicy authorizationPolicy, PolicyAuthorizationResult policyAuthorizationResult)
    {
        if (!policyAuthorizationResult.Succeeded)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        await _handler.HandleAsync(requestDelegate, httpContext, authorizationPolicy, policyAuthorizationResult);
    }
}
