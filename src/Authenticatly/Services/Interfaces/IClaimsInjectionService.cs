using System.Security.Claims;

namespace Authenticatly.Services.Interfaces;

public interface IClaimsInjectionService
{
    Task<List<Claim>> GetExtraClaimsForUserId(string userId);
}
