using Microsoft.AspNetCore.Authorization;

namespace Authenticatly.Authorization;
public class AuthenticatlyAuthorizeAttribute : AuthorizeAttribute
{
    public AuthenticatlyAuthorizeAttribute() : base(AuthenticatlyPolicyNames.AuthenticatlyPolicy)
    {

    }
}
