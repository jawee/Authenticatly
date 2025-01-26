using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authenticatly.Services;
public interface ITokenService
{
    string CreateTokenForUser<TUser>(TUser user, IList<Claim> claims) where TUser : IdentityUser;
    string GetUserEmailFromToken(string token);
    ClaimsPrincipal GetClaimsPrincipalFromToken(string token);
    bool TokenIsValid(string token);
}

public class TokenService : ITokenService
{
    private readonly ILogger<ITokenService> _logger;
    private readonly SymmetricSecurityKey _key;
    private readonly AuthenticatlyAuthOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenService(ILogger<ITokenService> logger, IOptions<AuthenticatlyAuthOptions> optionsAccessor, JwtSecurityTokenHandler tokenHandler)
    {
        _logger = logger;
        _options = optionsAccessor.Value;
        _key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_options.SymmetricSecurityKey));
        _tokenHandler = tokenHandler;
    }

    public string CreateTokenForUser<TUser>(TUser user, IList<Claim> claims) where TUser : IdentityUser
    {
        if (claims.Count == 0)
        {
            throw new ArgumentNullException(nameof(claims));
        }
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_options.TokenValidTimeInMinutes),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);

        return _tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal GetClaimsPrincipalFromToken(string token)
    {
        token = token.Replace("Bearer ", "");

        var parameters = new TokenValidationParameters
        {
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            RequireSignedTokens = true,
            IssuerSigningKey = _key,
            ValidateLifetime = true,
        };

        var claimsPrincipal = _tokenHandler.ValidateToken(token, parameters, out _);

        return claimsPrincipal;
    }

    public string GetUserEmailFromToken(string token)
    {
        token = token.Replace("Bearer ", "");

        var res = _tokenHandler.ReadJwtToken(token);

        var emailClaim = res.Claims.FirstOrDefault(x => x.Type == "email");

        if (emailClaim is null)
        {
            //TODO 1: Throw exception or return empty? Should be a custom exception at least.
            throw new KeyNotFoundException();
        }

        return emailClaim.Value;
    }

    public bool TokenIsValid(string token)
    {
        token = token.Replace("Bearer ", "");

        var parameters = new TokenValidationParameters
        {
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            RequireSignedTokens = true,
            IssuerSigningKey = _key,
            ValidateLifetime = true,
        };

        try
        {
            _ = _tokenHandler.ValidateToken(token, parameters, out var _);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("Token expired");
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in TokenIsValid");
            return false;
        }

        return true;
    }
}
