using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sisapi.domain.Entities;
using sisapi.domain.Enum;
using sisapi.infrastructure.Context.Core;

namespace sisapi.infrastructure.Services;

public class DatabaseSeeder
{
    private readonly CoreDbContext _context;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public DatabaseSeeder(CoreDbContext context, RoleManager<Role> roleManager, UserManager<User> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        await SeedControllerPermissionsAsync();
        await SeedAdminRoleAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedControllerPermissionsAsync()
    {
        // Define all controller names in the SISAPI module
        var controllerNames = new[]
        {
            "Auth",
            "Permission",
            "Role",
            "RolePermission",
            "Company",
            "User"
        };

        foreach (var controllerName in controllerNames)
        {
            var permissionCode = controllerName;
            
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == permissionCode && p.Module == Module.SISAPI);

            if (existingPermission == null)
            {
                var permission = new Permission
                {
                    Code = permissionCode,
                    Description = $"Controller permission for {controllerName}",
                    Module = Module.SISAPI,
                    TypePermission = TypePermission.ControllerAction,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Permissions.Add(permission);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedAdminRoleAsync()
    {
        const string adminRoleName = "Admin";

        var adminRoleExists = await _roleManager.RoleExistsAsync(adminRoleName);
        if (!adminRoleExists)
        {
            var adminRole = new Role
            {
                Name = adminRoleName,
                NormalizedName = adminRoleName.ToUpper(),
                Description = "Administrator role with full access to all SISAPI controllers",
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            await _roleManager.CreateAsync(adminRole);
        }

        // Get the admin role
        var role = await _roleManager.FindByNameAsync(adminRoleName);
        if (role == null) return;

        // Get all SISAPI controller permissions
        var permissions = await _context.Permissions
            .Where(p => p.Module == Module.SISAPI && p.TypePermission == TypePermission.ControllerAction && p.Active)
            .ToListAsync();

        foreach (var permission in permissions)
        {
            var existingRolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

            if (existingRolePermission == null)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    Read = true,
                    Write = true,
                    Update = true,
                    Delete = true,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RolePermissions.Add(rolePermission);
            }
            else if (!existingRolePermission.Read || !existingRolePermission.Write || 
                     !existingRolePermission.Update || !existingRolePermission.Delete)
            {
                // Update to full permissions if not already set
                existingRolePermission.Read = true;
                existingRolePermission.Write = true;
                existingRolePermission.Update = true;
                existingRolePermission.Delete = true;
                existingRolePermission.Active = true;
                existingRolePermission.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@sisapi.com";
        const string adminUserName = "admin";
        const string adminPassword = "Admin@123!";

        var existingUser = await _userManager.FindByEmailAsync(adminEmail);
        if (existingUser == null)
        {
            var adminUser = new User
            {
                UserName = adminUserName,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // Ensure admin user has the Admin role
            var isInRole = await _userManager.IsInRoleAsync(existingUser, "Admin");
            if (!isInRole)
            {
                await _userManager.AddToRoleAsync(existingUser, "Admin");
            }
        }
    }
}

