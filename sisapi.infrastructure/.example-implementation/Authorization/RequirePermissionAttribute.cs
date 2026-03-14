using Microsoft.AspNetCore.Authorization;

namespace sisconapi.infrastructure.Authorization;

/// <summary>
/// Authorization attribute that checks for specific controller action permission.
/// Format: Module-Controller:Action (e.g., SISCON-Accounts:Read, SISCON-Transactions:Write)
/// 
/// Usage:
/// [RequirePermission("SISCON", "Accounts", "Read")]  -> Requires SISCON-Accounts:Read permission
/// [RequirePermission("SISCON", "Accounts", "Write")] -> Requires SISCON-Accounts:Write permission
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission_";

    public RequirePermissionAttribute(string module, string controller, string action)
    {
        Policy = $"{PolicyPrefix}{module}-{controller}:{action}";
    }
}

