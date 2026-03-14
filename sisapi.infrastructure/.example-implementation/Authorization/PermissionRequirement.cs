using Microsoft.AspNetCore.Authorization;

namespace sisconapi.infrastructure.Authorization;

/// <summary>
/// Authorization requirement for permission-based authorization.
/// Validates permissions in the format: Module-Controller:Action
/// (e.g., SISCON-Accounts:Read, SISCON-Transactions:Write)
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

