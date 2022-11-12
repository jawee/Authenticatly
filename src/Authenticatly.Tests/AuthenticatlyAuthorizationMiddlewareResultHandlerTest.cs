using Authenticatly.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Authenticatly.Tests;

[TestClass]
public class AuthenticatlyAuthorizationMiddlewareResultHandlerTest
{

    [TestInitialize]
    public void Setup()
    {
    }

    [TestMethod]
    public async Task HandleAsync_ValidToken_HasSucceeded()
    {
        var handler = new AuthenticatlyAuthorizationMiddlewareResultHandler();

        var httpContext = new DefaultHttpContext();
        var method = (RequestDelegate)((_) => { return Task.CompletedTask; });
        var polRes = PolicyAuthorizationResult.Success();
        var authPolicy = new AuthorizationPolicy(new List<IAuthorizationRequirement> { new AuthenticatlyAuthorizeRequirement() }, new List<string>());
        await handler.HandleAsync(method, httpContext, authPolicy, polRes);

        Assert.IsTrue(httpContext.Response.StatusCode == StatusCodes.Status200OK, $"{httpContext.Response.StatusCode} is not {StatusCodes.Status200OK}");
    }

    [TestMethod]
    public async Task HandleAsync_ValidToken_HasFailed()
    {
        var handler = new AuthenticatlyAuthorizationMiddlewareResultHandler();

        var httpContext = new DefaultHttpContext();
        var method = (RequestDelegate)((_) => { return Task.CompletedTask; });
        var polRes = PolicyAuthorizationResult.Forbid();
        var authPolicy = new AuthorizationPolicy(new List<IAuthorizationRequirement> { new AuthenticatlyAuthorizeRequirement() }, new List<string>());
        await handler.HandleAsync(method, httpContext, authPolicy, polRes);

        Assert.IsTrue(httpContext.Response.StatusCode == StatusCodes.Status401Unauthorized, $"{httpContext.Response.StatusCode} is not {StatusCodes.Status401Unauthorized}");
    }
}