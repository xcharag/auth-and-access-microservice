using Microsoft.AspNetCore.Authorization;

namespace sisapi.infrastructure.Authorization;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission_";

    public RequirePermissionAttribute(string module, string controller, string action)
    {
        Policy = $"{PolicyPrefix}{module}-{controller}:{action}";
    }
}

