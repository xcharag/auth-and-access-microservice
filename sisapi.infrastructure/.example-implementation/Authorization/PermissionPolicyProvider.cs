using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace sisconapi.infrastructure.Authorization;

/// <summary>
/// Custom policy provider that dynamically creates authorization policies
/// based on permission names. This enables the [RequirePermission] attribute
/// to work without pre-registering every possible policy.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => 
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => 
        _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if this is a permission-based policy
        if (policyName.StartsWith(RequirePermissionAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring(RequirePermissionAttribute.PolicyPrefix.Length);
            
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
            
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to the default policy provider for other policies
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}

