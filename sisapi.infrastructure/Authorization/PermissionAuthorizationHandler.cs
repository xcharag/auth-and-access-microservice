using Microsoft.AspNetCore.Authorization;

namespace sisapi.infrastructure.Authorization;

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

