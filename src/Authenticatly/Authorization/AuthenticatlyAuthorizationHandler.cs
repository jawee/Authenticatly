using Authenticatly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Authenticatly.Authorization;

public class AuthenticatlyAuthorizationHandler : AuthorizationHandler<AuthenticatlyAuthorizeRequirement>
{
    private readonly ILogger<AuthenticatlyAuthorizationHandler> _logger;
    private readonly ITokenService _service;
    public AuthenticatlyAuthorizationHandler(ILogger<AuthenticatlyAuthorizationHandler> logger, ITokenService service)
    {
        _logger = logger;
        _service = service;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthenticatlyAuthorizeRequirement requirement)
    {
        _logger.LogDebug("In HandleRequirementAsync()");
        var httpContext = context.Resource as DefaultHttpContext;
        if (httpContext is not null)
        {
            var token = httpContext.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(token) && _service.TokenIsValid(token))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        context.Fail();
        return Task.CompletedTask;
    }
}
