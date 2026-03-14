using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sisapi.domain.Abstractions;
using sisapi.domain.Entities;
using sisapi.infrastructure.Context.Core;

namespace sisapi.application.Implementations;

/// <summary>
/// Implementation of IPermissionVerifier that checks permissions from database
/// </summary>
public class PermissionVerifier(UserManager<User> userManager, CoreDbContext context) : IPermissionVerifier
{
    public async Task<bool> HasPermissionAsync(int userId, string module, string controller, string action)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.Active || user.IsDeleted)
        {
            return false;
        }

        // Build the required permission code: Module-Controller:Action
        var requiredPermission = $"{module}-{controller}:{action}";

        // Get user's roles
        var roles = await userManager.GetRolesAsync(user);

        // Get permissions for user's roles
        var permissions = await context.RolePermissions
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => roles.Contains(rp.Role.Name!) && rp.Active && rp.Permission.Active)
            .Select(rp => new {
                rp.Permission.Code,
                rp.Permission.Module,
                rp.Read,
                rp.Write,
                rp.Update,
                rp.Delete
            })
            .ToListAsync();

        var permissionClaims = new List<string>();

        foreach (var p in permissions)
        {
            // Format: SISAPI-Permission:Read, SISAPI-Permission:Write, etc.
            if (p.Read) permissionClaims.Add($"{p.Module}-{p.Code}:Read");
            if (p.Write) permissionClaims.Add($"{p.Module}-{p.Code}:Write");
            if (p.Update) permissionClaims.Add($"{p.Module}-{p.Code}:Update");
            if (p.Delete) permissionClaims.Add($"{p.Module}-{p.Code}:Delete");
        }

        return permissionClaims.Distinct().Contains(requiredPermission);
    }
}

