﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Authenticatly.Authorization;

public class AuthenticatlyPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public AuthenticatlyPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new(options);
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(AuthenticatlyPolicyNames.AuthenticatlyPolicy, StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new AuthenticatlyAuthorizeRequirement());
            return Task.FromResult(policy.Build());
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
