using Authenticatly.Authorization;
using Authenticatly.Services;
using Authenticatly.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;

namespace Authenticatly.Tests;

[TestClass]
public class AuthenticatlyClaimsMiddlewareTests
{

    [TestMethod]
    public async Task AuthenticatlyClaimsMiddlewareTest()
    {
        var claimsList = new List<Claim> { new(ClaimTypes.Email, "asdf@asdf.se"), new("CustomerId", "1") };
        var claimsIdentity = new ClaimsIdentity(claimsList);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(x => x.GetClaimsPrincipalFromToken(It.IsAny<string>())).Returns(claimsPrincipal);

        var httpContext = new DefaultHttpContext();
        var req = httpContext.Request;

        req.Headers.Append("Authorization", "Bearer asdf");

        RequestDelegate next = async (hc) => { await Task.CompletedTask; };
        var eccm = new AuthenticatlyClaimsMiddleware(next, tokenServiceMock.Object);
        await eccm.Invoke(httpContext);

        var res = httpContext.Items.TryGetValue(AuthenticatlyAuthConstants.AUTHORIZED_ATTRIBUTES_KEY, out var value);

        Assert.AreEqual(true, res);
        if (value is not Dictionary<string, string> authorizedAttributes)
        {
            Assert.Fail($"value is not {nameof(Dictionary<string, string>)}");
            return;
        }

        Assert.AreEqual("asdf@asdf.se", authorizedAttributes[ClaimTypes.Email]);
        Assert.AreEqual("1", authorizedAttributes["CustomerId"]);
    }
}
