using Authenticatly.Services.Interfaces;
using System.Security.Claims;

namespace ExampleApi.Services;

public class ClaimsInjectionService : IClaimsInjectionService
{
    public Task<List<Claim>> GetExtraClaimsForUserId(string userId)
    {
        var claims = new List<Claim>() {
            new("TestType", "TestValue"),
            new("UserId", userId) 
        };
        return Task.FromResult(claims);
    }
}
