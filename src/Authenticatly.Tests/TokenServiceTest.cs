using Authenticatly.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Authenticatly.Tests;

[TestClass]
public class TokenServiceTest
{
    private TokenService _service = null!;
    private IdentityUser _user = null!;

    [TestInitialize]
    public void Setup()
    {
        var logger = new Mock<ILogger<TokenService>>();
        var authOpts = new AuthenticatlyAuthOptions
        {
            SymmetricSecurityKey = "mysupersecrettestkeythatmustbe32charactersormore",
            Audience = "TestAudience",
            Issuer = "TestIssuer",
            TokenValidTimeInMinutes = 10
        };
        _service = new(logger.Object, Options.Create(authOpts), new JwtSecurityTokenHandler());
        _user = new()
        {
            UserName = "some@email.com",
            Email = "some@email.com"
        };
    }

    [TestMethod]
    public void CreateToken_ReturnsToken()
    {

        var claims = new List<Claim>()
        {
            new(ClaimTypes.Email, _user.Email!)
        };

        var token = _service.CreateTokenForUser(_user, claims);

        Assert.IsNotNull(token, $"Expected token to be set, got '{token}'");
    }

    [TestMethod]
    public void CreateToken_NoEmail_ThrowsException()
    {
        _user.Email = null;

        Assert.ThrowsExactly<ArgumentNullException>(() => _service.CreateTokenForUser(_user, []));
    }

    [TestMethod]
    public void GetEmailFromToken_EmailClaimInToken_ReturnsEmail()
    {
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InNvbWVAZW1haWwuY29tIiwibmJmIjoxNjYyMzcwMTQzLCJleHAiOjE2NjIzNzA0NDMsImlhdCI6MTY2MjM3MDE0MywiaXNzIjoiRWFzdENvYXN0IiwiYXVkIjoiQXVkaWVuY2UifQ.9yiQ3Z61tqP-4aoI2RK5u99j71WgIEQh2V4MZOGRsWQ";

        var email = _service.GetUserEmailFromToken(token);

        Assert.AreEqual(_user.Email, email, $"Expected to get '{_user.Email}', got '{email}'");
    }

    [TestMethod]
    public void GetEmailFromToken_NoEmailClaim_ThrowsException()
    {
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE2NjI1NDcwNjAsImV4cCI6MTY2MjU0NzM1OSwiaWF0IjoxNjYyNTQ3MDU5LCJpc3MiOiJFYXN0Q29hc3QiLCJhdWQiOiJBdWRpZW5jZSJ9.VqK0dUWVN5WSsWoKRDrvL4yUQczLrnXiQYWsR413Sok";

        Assert.ThrowsExactly<KeyNotFoundException>(() => _service.GetUserEmailFromToken(token));
    }

    [TestMethod]
    public void TokenIsValid_ValidToken_ReturnsTrue()
    {
        var claims = new List<Claim>()
        {
            new(ClaimTypes.Email, _user.Email!)
        };

        var token = _service.CreateTokenForUser(_user, claims);

        var tokenIsValid = _service.TokenIsValid(token);

        Assert.IsTrue(tokenIsValid, $"Expected valid token, got '{tokenIsValid}'");
    }

    [TestMethod]
    public void TokenIsValid_ExpiredToken_ReturnsFalse()
    {
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE2NjI1NDcwNjAsImV4cCI6MTY2MjU0NzM1OSwiaWF0IjoxNjYyNTQ3MDU5LCJpc3MiOiJFYXN0Q29hc3QiLCJhdWQiOiJBdWRpZW5jZSJ9.VqK0dUWVN5WSsWoKRDrvL4yUQczLrnXiQYWsR413Sok";

        var tokenIsValid = _service.TokenIsValid(token);

        Assert.IsFalse(tokenIsValid, $"Expected invalid token, got '{tokenIsValid}'");
    }

    [TestMethod]
    public void TokenIsValid_EmptyToken_ReturnsFalse()
    {
        var tokenIsValid = _service.TokenIsValid("");

        Assert.IsFalse(tokenIsValid, $"Expected invalid token, got '{tokenIsValid}'");
    }
}
