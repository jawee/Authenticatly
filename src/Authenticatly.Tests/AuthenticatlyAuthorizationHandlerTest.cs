using Authenticatly.Authorization;
using Authenticatly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;

namespace Authenticatly.Tests;

[TestClass]
public class AuthenticatlyAuthorizationHandlerTest
{
    private Mock<ILogger<AuthenticatlyAuthorizationHandler>> _logger;
    private Mock<ITokenService> _tokenService;

    [TestInitialize]
    public void Setup()
    {
        _logger = new Mock<ILogger<AuthenticatlyAuthorizationHandler>>();
        _tokenService = new Mock<ITokenService>();
    }
    [TestMethod]
    public async Task HandleAsync_ValidToken_HasSucceeded()
    {
        _tokenService.Setup(x => x.TokenIsValid(It.IsAny<string>())).Returns(true);
        var handler = new AuthenticatlyAuthorizationHandler(_logger.Object, _tokenService.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer asdfasdf"));

        var authorizationHandlerContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { new AuthenticatlyAuthorizeRequirement() }, new ClaimsPrincipal(), httpContext);

        await handler.HandleAsync(authorizationHandlerContext);

        Assert.IsTrue(authorizationHandlerContext.HasSucceeded, $"HasSucceeded is not 'true', got {authorizationHandlerContext.HasSucceeded}");
    }

    [TestMethod]
    public async Task HandleAsync_InvalidToken_HasFailed()
    {
        _tokenService.Setup(x => x.TokenIsValid(It.IsAny<string>())).Returns(false);

        var handler = new AuthenticatlyAuthorizationHandler(_logger.Object, _tokenService.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer asdfasdf"));

        var authorizationHandlerContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { new AuthenticatlyAuthorizeRequirement() }, new ClaimsPrincipal(), httpContext);

        await handler.HandleAsync(authorizationHandlerContext);

        Assert.IsFalse(authorizationHandlerContext.HasSucceeded, $"HasSucceeded is not 'false', got {authorizationHandlerContext.HasSucceeded}");
        Assert.IsTrue(authorizationHandlerContext.HasFailed, $"HasFailed is not 'true', got {authorizationHandlerContext.HasFailed}");
    }

    [TestMethod]
    public async Task HandleAsync_NoToken_HasFailed()
    {
        _tokenService.Setup(x => x.TokenIsValid("")).Returns(false);
        _tokenService.Setup(x => x.TokenIsValid(It.IsAny<string>())).Returns(true);

        var handler = new AuthenticatlyAuthorizationHandler(_logger.Object, _tokenService.Object);
        var httpContext = new DefaultHttpContext();

        var authorizationHandlerContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { new AuthenticatlyAuthorizeRequirement() }, new ClaimsPrincipal(), httpContext);

        await handler.HandleAsync(authorizationHandlerContext);

        Assert.IsFalse(authorizationHandlerContext.HasSucceeded, $"HasSucceeded is not 'false', got {authorizationHandlerContext.HasSucceeded}");
        Assert.IsTrue(authorizationHandlerContext.HasFailed, $"HasFailed is not 'true', got {authorizationHandlerContext.HasFailed}");
    }
}