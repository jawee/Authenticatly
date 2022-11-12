using Authenticatly.IntegrationTests.Utils;
using Authenticatly.Requests;
using ExampleApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;

namespace Authenticatly.IntegrationTests;

[TestClass]
public class AuthV1EndpointsTests
{
    [TestInitialize]
    public void Setup()
    {
    }

    [TestMethod]
    public async Task POST_Login_RefreshTokenTwice_Returns200OK()
    {
        await using var application = new TestWebApplicationFactory<WebMarker>();
        var client = application.CreateClient();

        var resp1 = await client.PostAsJsonAsync("/auth/v1/login", new AuthenticateRequest { Email = TestConstants.Username, Password = TestConstants.Password });

        var responseBody = await resp1.Content.ReadAsStringAsync();
        var responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

        var resp2 = await client.PostAsJsonAsync("/auth/v1/login", new AuthenticateRequest { Email = TestConstants.Username, RefreshToken = responseDict["refreshToken"] as string });

        responseBody = await resp2.Content.ReadAsStringAsync();
        responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);


        var resp3 = await client.PostAsJsonAsync("/auth/v1/login", new AuthenticateRequest { Email = TestConstants.Username, RefreshToken = responseDict["refreshToken"] as string });
        responseBody = await resp3.Content.ReadAsStringAsync();
        responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

        Assert.AreEqual(HttpStatusCode.OK, resp3.StatusCode);
        Assert.IsTrue(responseDict.ContainsKey("accessToken"));
        Assert.IsTrue(responseDict.ContainsKey("tokenType"));
        Assert.IsTrue(responseDict.ContainsKey("expiresIn"));
        Assert.IsTrue(responseDict.ContainsKey("scope"));
        Assert.IsTrue(responseDict.ContainsKey("refreshToken"));
    }

    [TestMethod]
    public async Task POST_Login_NoMfa_Returns200OK()
    {
        await using var application = new TestWebApplicationFactory<WebMarker>();
        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/v1/login", new AuthenticateRequest { Email = TestConstants.Username, Password = TestConstants.Password });

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

        if (responseDict is null)
        {
            Assert.Fail($"Failed to parse response to dictionary");
        }

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(responseDict.ContainsKey("accessToken"));
        Assert.IsTrue(responseDict.ContainsKey("tokenType"));
        Assert.IsTrue(responseDict.ContainsKey("expiresIn"));
        Assert.IsTrue(responseDict.ContainsKey("scope"));
        Assert.IsTrue(responseDict.ContainsKey("refreshToken"));
    }

    [TestMethod]
    public async Task POST_Login_Mfa_Returns403Forbidden()
    {
        await using var application = new TestWebApplicationFactory<WebMarker>();
        var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/v1/login", new AuthenticateRequest { Email = TestConstants.MfaUsername, Password = TestConstants.Password });
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

        if (responseDict is null)
        {
            Assert.Fail($"Failed to parse response to dictionary");
        }

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.IsTrue(responseDict.ContainsKey("error"));
        Assert.IsTrue(responseDict.ContainsKey("mfaToken"));
    }

    [TestMethod]
    public async Task POST_Challenge_sms_Returns200OK()
    {
        await using var application = new TestWebApplicationFactory<WebMarker>();
        var client = application.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/auth/v1/login", new AuthenticateRequest { Email = TestConstants.MfaUsername, Password = TestConstants.Password });

        var loginRespBody = await loginResponse.Content.ReadAsStringAsync();
        var loginRespDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(loginRespBody);

        var challengeResponse = await client.PostAsJsonAsync("/auth/v1/challenge", new ChallengeRequest { ChallengeType = "sms", MfaToken = loginRespDict["mfaToken"] as string });
        var challengeResponseBody = await challengeResponse.Content.ReadAsStringAsync();

        var challengeResponseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(challengeResponseBody);
        if (challengeResponseDict is null)
        {
            Assert.Fail($"Failed to parse response to dictionary");
            return;
        }

        Assert.AreEqual(HttpStatusCode.OK, challengeResponse.StatusCode);
        Assert.IsTrue(challengeResponseDict.ContainsKey("challengeType"));
        Assert.IsTrue(challengeResponseDict.ContainsKey("bindingMethod"));
        Assert.IsTrue(challengeResponseDict.ContainsKey("oobCode"));
        Assert.IsTrue(challengeResponseDict.ContainsKey("additionalProperties"));
        Assert.IsTrue(challengeResponseBody.Contains("phonenumber"));
    }

    [TestMethod]
    public async Task GET_ProtectedWithoutToken_Returns401Unauthorized()
    {
        await using var application = new TestWebApplicationFactory<WebMarker>();
        var client = application.CreateClient();

        var getProtectedResponse = await client.GetAsync("/protected");

        Assert.AreEqual(HttpStatusCode.Unauthorized, getProtectedResponse.StatusCode);
    }

    [TestMethod]
    public async Task GET_ProtectedWithToken_Returns200OK()
    {
        await using var application = new TestWebApplicationFactory<WebMarker>();
        var client = application.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/v1/login", new AuthenticateRequest { Email = TestConstants.Username, Password = TestConstants.Password });

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {responseDict["accessToken"] as string}");
        var getProtectedResponse = await client.GetAsync("/protected");

        Assert.AreEqual(HttpStatusCode.OK, getProtectedResponse.StatusCode);
    }
}