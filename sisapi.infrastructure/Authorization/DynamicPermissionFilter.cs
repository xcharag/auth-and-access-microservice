using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using sisapi.domain.Abstractions;
using sisapi.domain.Enum;
using System.Security.Claims;

namespace sisapi.infrastructure.Authorization;


public class DynamicPermissionFilter : IAsyncAuthorizationFilter
{
    private readonly Module _module;
    private readonly ILogger<DynamicPermissionFilter> _logger;
    private readonly IPermissionVerifier _permissionVerifier;

    public DynamicPermissionFilter(
        IPermissionVerifier permissionVerifier,
        Module module = Module.SISAPI, 
        ILogger<DynamicPermissionFilter>? logger = null)
    {
        _permissionVerifier = permissionVerifier;
        _module = module;
        _logger = logger ?? new LoggerFactory().CreateLogger<DynamicPermissionFilter>();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var controllerName = context.RouteData.Values["controller"]?.ToString() ?? string.Empty;

        var httpMethod = context.HttpContext.Request.Method.ToUpper();
        var action = httpMethod switch
        {
            "GET" => "Read",
            "POST" => "Write",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Read"
        };

        _logger.LogInformation("Checking permission: {Module}-{Controller}:{Action} for user {UserId}", 
            _module, controllerName, action, userId);

        var hasPermission = await _permissionVerifier.HasPermissionAsync(userId, _module.ToString(), controllerName, action);

        if (!hasPermission)
        {
            _logger.LogWarning("Permission denied - {Module}-{Controller}:{Action} for user {UserId}", 
                _module, controllerName, action, userId);
            context.Result = new ForbidResult();
            return;
        }

        _logger.LogInformation("Permission granted: {Module}-{Controller}:{Action} for user {UserId}", 
            _module, controllerName, action, userId);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class DynamicPermissionAttribute : TypeFilterAttribute
{
    public DynamicPermissionAttribute(Module module = Module.SISAPI) 
        : base(typeof(DynamicPermissionFilter))
    {
        Arguments = new object[] { module };
    }
}

