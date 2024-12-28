using System.Security.Claims;
using System.Text.Json;
using Authenticatly.Exceptions;
using Authenticatly.Services;
using Authenticatly.Responses;
using Authenticatly.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Authenticatly.Requests;

namespace Authenticatly.Tests;

[TestClass]
public class LoginServiceTests
{
    private const string EMAIL = "asdf@asdf.se";
    private const string PASSWORD = "asdfs";
    private const string TOKEN = "asdf";
    private const string ROLENAME = "role";
    private const string USER_ID = "7b4fcf69-a621-43c1-a5a5-fe2ce9375f78";

    private Mock<ITokenService> _tokenServiceMock = null!;
    private Mock<UserManager<IdentityUser>> _userManagerMock = null!;
    private AuthenticateRequest _authReq = null!;
    private IdentityUser _user = null!;
    private IOptions<AuthenticatlyAuthOptions> _optionsAccessor = null!;
    private ILoginService _loginService = null!;
    private Mock<IMfaTokenService> _mfaTokenServiceMock = null!;
    private Mock<IClaimsInjectionService> _claimsInjectionServiceMock = null!;
    private Mock<ISendSmsService> _sendSmsServiceMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _userManagerMock = MockUserManager<IdentityUser>();
        _mfaTokenServiceMock = new Mock<IMfaTokenService>();
        _claimsInjectionServiceMock = new Mock<IClaimsInjectionService>();
        _sendSmsServiceMock = new Mock<ISendSmsService>();
        _authReq = new(EMAIL, PASSWORD);
        _user = new() { Id = USER_ID, UserName = EMAIL };

        _claimsInjectionServiceMock.Setup(s => s.GetExtraClaimsForUserId(It.IsAny<string>())).ReturnsAsync(new List<Claim>());

        _loginService = new LoginService(_mfaTokenServiceMock.Object, _claimsInjectionServiceMock.Object);

            var authOpts = new AuthenticatlyAuthOptions
            {
                SymmetricSecurityKey = "mysupersecrettestkey",
                Audience = "TestAudience",
                Issuer = "TestIssuer",
                TokenValidTimeInMinutes = 10,
            };
            _optionsAccessor = Options.Create(authOpts);
        }

    [TestMethod]
    public async Task Login_NoExistingUser_ThrowsLoginFailedException()
    {
        await Assert.ThrowsExceptionAsync<LoginFailedException>(async () => await _loginService.Login(_authReq, _userManagerMock.Object, _tokenServiceMock.Object, _optionsAccessor));
    }

    [TestMethod]
    public async Task Login_UserHasRequiredRoles_ReturnsToken()
    {
        var tokenResponse = new TokenResponse(TOKEN, "Bearer", 600, Array.Empty<string>(), "");
        var tokenResponseStr = JsonSerializer.Serialize(tokenResponse);
        _tokenServiceMock.Setup(t => t.CreateTokenForUser(It.IsAny<IdentityUser>(), It.IsAny<List<Claim>>())).Returns(TOKEN);
        var userManagerMock = SetupUserManager(_user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(_user, PASSWORD)).ReturnsAsync(true).Verifiable();
        userManagerMock.Setup(m => m.FindByEmailAsync(EMAIL)).ReturnsAsync(_user).Verifiable();
        userManagerMock.Setup(m => m.GetClaimsAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<Claim> { new Claim(ClaimTypes.Email, EMAIL)});
        userManagerMock.Setup(m => m.GenerateUserTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");
        userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string> { ROLENAME });

            var authOpts = new AuthenticatlyAuthOptions
            {
                SymmetricSecurityKey = "mysupersecrettestkey",
                Audience = "TestAudience",
                Issuer = "TestIssuer",
                TokenValidTimeInMinutes = 10,
                AllowedRolesString = ROLENAME
            };
            _optionsAccessor = Options.Create(authOpts);
                
            var res = await _loginService.Login(_authReq, userManagerMock.Object, _tokenServiceMock.Object, _optionsAccessor);

        var resStr = JsonSerializer.Serialize(res);

        Assert.AreEqual(tokenResponseStr, resStr, $"Expected token to be '{tokenResponseStr}', got '{resStr}'.");
    }

    [TestMethod]
    public async Task Login_UserHasNoRoles_ThrowsUnauthorizedException()
    {

        var tokenResponse = new TokenResponse(TOKEN, "Bearer", 600, Array.Empty<string>(), "");
        var tokenResponseStr = JsonSerializer.Serialize(tokenResponse);
        _tokenServiceMock.Setup(t => t.CreateTokenForUser(It.IsAny<IdentityUser>(), It.IsAny<List<Claim>>())).Returns(TOKEN);
        var userManagerMock = SetupUserManager(_user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(_user, PASSWORD)).ReturnsAsync(true).Verifiable();
        userManagerMock.Setup(m => m.FindByEmailAsync(EMAIL)).ReturnsAsync(_user).Verifiable();
        userManagerMock.Setup(m => m.GetClaimsAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<Claim> { new Claim(ClaimTypes.Email, EMAIL)});
        userManagerMock.Setup(m => m.GenerateUserTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");
        userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string> {});

            var authOpts = new AuthenticatlyAuthOptions
            {
                SymmetricSecurityKey = "mysupersecrettestkey",
                Audience = "TestAudience",
                Issuer = "TestIssuer",
                TokenValidTimeInMinutes = 10,
                AllowedRolesString = ROLENAME
            };
            _optionsAccessor = Options.Create(authOpts);

        await Assert.ThrowsExceptionAsync<UnauthorizedException>(async () => await _loginService.Login(_authReq, userManagerMock.Object, _tokenServiceMock.Object, _optionsAccessor));
    }


    [TestMethod]
    public async Task Login_UserHasWrongRole_ThrowsUnauthorizedException()
    {

        var tokenResponse = new TokenResponse(TOKEN, "Bearer", 600, Array.Empty<string>(), "");
        var tokenResponseStr = JsonSerializer.Serialize(tokenResponse);
        _tokenServiceMock.Setup(t => t.CreateTokenForUser(It.IsAny<IdentityUser>(), It.IsAny<List<Claim>>())).Returns(TOKEN);
        var userManagerMock = SetupUserManager(_user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(_user, PASSWORD)).ReturnsAsync(true).Verifiable();
        userManagerMock.Setup(m => m.FindByEmailAsync(EMAIL)).ReturnsAsync(_user).Verifiable();
        userManagerMock.Setup(m => m.GetClaimsAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<Claim> { new Claim(ClaimTypes.Email, EMAIL)});
        userManagerMock.Setup(m => m.GenerateUserTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");
        userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string> { "wrongRole" });

            var authOpts = new AuthenticatlyAuthOptions
            {
                SymmetricSecurityKey = "mysupersecrettestkey",
                Audience = "TestAudience",
                Issuer = "TestIssuer",
                TokenValidTimeInMinutes = 10,
                AllowedRolesString = ROLENAME
            };
            _optionsAccessor = Options.Create(authOpts);

        await Assert.ThrowsExceptionAsync<UnauthorizedException>(async () => await _loginService.Login(_authReq, userManagerMock.Object, _tokenServiceMock.Object, _optionsAccessor));
    }

    [TestMethod]
    public async Task Login_ExistingUser_ReturnsToken()
    {
        var tokenResponse = new TokenResponse(TOKEN, "Bearer", 600, Array.Empty<string>(), "");
        var tokenResponseStr = JsonSerializer.Serialize(tokenResponse);
        _tokenServiceMock.Setup(t => t.CreateTokenForUser(It.IsAny<IdentityUser>(), It.IsAny<List<Claim>>())).Returns(TOKEN);
        var userManagerMock = SetupUserManager(_user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(_user, PASSWORD)).ReturnsAsync(true).Verifiable();
        userManagerMock.Setup(m => m.FindByEmailAsync(EMAIL)).ReturnsAsync(_user).Verifiable();
        userManagerMock.Setup(m => m.GetClaimsAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<Claim> { new Claim(ClaimTypes.Email, EMAIL)});
        userManagerMock.Setup(m => m.GenerateUserTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");

        var res = await _loginService.Login(_authReq, userManagerMock.Object, _tokenServiceMock.Object, _optionsAccessor);

        var resStr = JsonSerializer.Serialize(res);

        Assert.AreEqual(tokenResponseStr, resStr, $"Expected token to be '{tokenResponseStr}', got '{resStr}'.");
    }

    [TestMethod]
    public async Task Login_ExistingUser_TwoFactorRequired_InvalidOtp_ThrowsOtpInvalidException()
    {
        var authReq = new AuthenticateRequest(null, null, null, "MfaToken", null, null, null);
        _user.TwoFactorEnabled = true;
        _tokenServiceMock.Setup(t => t.CreateTokenForUser(It.IsAny<IdentityUser>(), It.IsAny<List<Claim>>())).Returns("asdf");
        var userManagerMock = SetupUserManager(_user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(_user, PASSWORD)).ReturnsAsync(true).Verifiable();
        userManagerMock.Setup(m => m.FindByEmailAsync(EMAIL)).ReturnsAsync(_user).Verifiable();
        userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(_user).Verifiable();
        userManagerMock.Setup(m => m.VerifyTwoFactorTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false).Verifiable();

        _mfaTokenServiceMock.Setup(m => m.GetUserIdFromMfaToken(It.IsAny<string>())).ReturnsAsync("userid");

        await Assert.ThrowsExceptionAsync<OtpInvalidException>(async () => await _loginService.Login(authReq, userManagerMock.Object, _tokenServiceMock.Object, _optionsAccessor));
    }

    [TestMethod]
    public async Task Login_ExistingUser_TwoFactorRequired_NoOtp_ThrowsMfaEnabledException()
    {
        _user.TwoFactorEnabled = true;
        var userManagerMock = SetupUserManager(_user);
        userManagerMock.Setup(m => m.CheckPasswordAsync(_user, PASSWORD)).ReturnsAsync(true).Verifiable();
        userManagerMock.Setup(m => m.FindByEmailAsync(EMAIL)).ReturnsAsync(_user).Verifiable();

        var authReq = new AuthenticateRequest(EMAIL, PASSWORD, null, null, null, null);

        await Assert.ThrowsExceptionAsync<MfaEnabledException>(async () => await _loginService.Login(authReq, userManagerMock.Object, _tokenServiceMock.Object, _optionsAccessor));
    }


    [TestMethod]
    public async Task Login_ExistingUser_TwoFactorRequired_ValidOtp_ReturnsToken()
    {
        var authReq = new AuthenticateRequest(null, null, null, "MfaToken", null, "1234", null);
        var tokenResponse = new TokenResponse(TOKEN, "Bearer", 600, Array.Empty<string>(), "");
        var tokenResponseStr = JsonSerializer.Serialize(tokenResponse);
        _user.TwoFactorEnabled = true;
        _tokenServiceMock.Setup(t => t.CreateTokenForUser(It.IsAny<IdentityUser>(), It.IsAny<List<Claim>>())).Returns(TOKEN);
        var userManagerMock = SetupUserManager(_user);
        userManagerMock.Setup(m => m.VerifyTwoFactorTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true).Verifiable();
        userManagerMock.Setup(m => m.GetClaimsAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<Claim> { new Claim(ClaimTypes.Email, EMAIL)});
        userManagerMock.Setup(m => m.GenerateUserTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("");
        userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(_user).Verifiable();

        _mfaTokenServiceMock.Setup(m => m.GetUserIdFromMfaToken(It.IsAny<string>())).ReturnsAsync("userid");
        var res = await _loginService.Login(authReq, userManagerMock.Object, _tokenServiceMock.Object, _optionsAccessor);

        var resStr = JsonSerializer.Serialize(res);

        Assert.AreEqual(tokenResponseStr, resStr, $"Expected token to be '{tokenResponseStr}', got '{resStr}'.");
    }

    [TestMethod]
    public async Task Challenge_EmptyUserId_ThrowsUnauthorizedException()
    {
        var req = new ChallengeRequest();
        _mfaTokenServiceMock.Setup(s => s.GetUserIdFromMfaToken(It.IsAny<string>())).ReturnsAsync("");

        await Assert.ThrowsExceptionAsync<UnauthorizedException>(async () => await _loginService.Challenge(req, _userManagerMock.Object, _mfaTokenServiceMock.Object, _sendSmsServiceMock.Object));
    }

    [TestMethod]
    public async Task Challenge_UserNotFound_ThrowsUnauthorizedException()
    {
        var req = new ChallengeRequest();
        _mfaTokenServiceMock.Setup(s => s.GetUserIdFromMfaToken(It.IsAny<string>())).ReturnsAsync(USER_ID);

        await Assert.ThrowsExceptionAsync<UnauthorizedException>(async () => await _loginService.Challenge(req, _userManagerMock.Object, _mfaTokenServiceMock.Object, _sendSmsServiceMock.Object));
    }

    [TestMethod]
    public async Task Challenge_EmptyChallengeType_ThrowsUnsupportedChallengeTypeException()
    {
        var req = new ChallengeRequest
        {
            ChallengeType = "",
            MfaToken = "asdf"
        };
        _mfaTokenServiceMock.Setup(s => s.GetUserIdFromMfaToken(It.IsAny<string>())).ReturnsAsync(USER_ID);
        _userManagerMock.Setup(s => s.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser { Id = USER_ID });

        await Assert.ThrowsExceptionAsync<UnsupportedChallengeTypeException>(async () => await _loginService.Challenge(req, _userManagerMock.Object, _mfaTokenServiceMock.Object, _sendSmsServiceMock.Object));
    }
    [TestMethod]
    public async Task Challenge_UnknownChallengeType_ThrowsUnsupportedChallengeTypeException()
    {
        var req = new ChallengeRequest
        {
            ChallengeType = "ThisShouldNotExist",
            MfaToken = "asdf"
        };
        _mfaTokenServiceMock.Setup(s => s.GetUserIdFromMfaToken(It.IsAny<string>())).ReturnsAsync(USER_ID);
        _userManagerMock.Setup(s => s.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser { Id = USER_ID });

        await Assert.ThrowsExceptionAsync<UnsupportedChallengeTypeException>(async () => await _loginService.Challenge(req, _userManagerMock.Object, _mfaTokenServiceMock.Object, _sendSmsServiceMock.Object));
    }

    [TestMethod]
    public async Task Challenge_SmsCouldNotBeSent_ThrowsForbiddenException()
    {
        var req = new ChallengeRequest
        {
            ChallengeType = "sms",
            MfaToken = "asdf"
        };

        _mfaTokenServiceMock.Setup(s => s.GetUserIdFromMfaToken(It.IsAny<string>())).ReturnsAsync(USER_ID);
        _userManagerMock.Setup(s => s.FindByIdAsync(USER_ID)).ReturnsAsync(new IdentityUser { Id = USER_ID, PhoneNumber = "12345678" });
        _sendSmsServiceMock.Setup(s => s.SendSms(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _userManagerMock.Setup(s => s.GenerateTwoFactorTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync("token");

        await Assert.ThrowsExceptionAsync<ForbiddenException>(async () => await _loginService.Challenge(req, _userManagerMock.Object, _mfaTokenServiceMock.Object, _sendSmsServiceMock.Object));
    }

    [TestMethod]
    public async Task Challenge_ReturnsChallengeResponse()
    {
        var req = new ChallengeRequest
        {
            ChallengeType = "sms",
            MfaToken = "asdf"
        };

        _mfaTokenServiceMock.Setup(s => s.GetUserIdFromMfaToken(It.IsAny<string>())).ReturnsAsync(USER_ID);
        _userManagerMock.Setup(s => s.FindByIdAsync(USER_ID)).ReturnsAsync(new IdentityUser { Id = USER_ID, PhoneNumber = "12345678" });
        _sendSmsServiceMock.Setup(s => s.SendSms(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _userManagerMock.Setup(s => s.GenerateTwoFactorTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync("token");

        var res = await _loginService.Challenge(req, _userManagerMock.Object, _mfaTokenServiceMock.Object, _sendSmsServiceMock.Object);

        Assert.AreEqual("prompt", res.BindingMethod);
        Assert.AreEqual("sms", res.ChallengeType);
        Assert.AreEqual("5678", res.AdditionalProperties["phonenumber"]);
    }
    private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
        return mgr;
    }

    private static Mock<UserManager<IdentityUser>> SetupUserManager(IdentityUser user)
    {
        var manager = MockUserManager<IdentityUser>();
        manager.Setup(m => m.FindByNameAsync(user.UserName!)).ReturnsAsync(user);
        manager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        manager.Setup(m => m.GetUserIdAsync(user)).ReturnsAsync(user.Id.ToString());
        manager.Setup(m => m.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
        return manager;
    }
}
