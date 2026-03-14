using Microsoft.AspNetCore.Authorization;

namespace sisconapi.infrastructure.Authorization;

/// <summary>
/// Handler for permission-based authorization.
/// Validates that the user has the required permission claim from the JWT token.
/// The JWT token should contain "Permission" claims issued by the auth microservice.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask;
        }

        // Check if user has the required permission claim
        // The auth microservice includes permissions in the JWT token as "Permission" claims
        var permissions = context.User.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

