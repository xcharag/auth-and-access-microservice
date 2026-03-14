using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using sisconapi.domain.Enum;
using sisconapi.infrastructure.Services;

namespace sisconapi.infrastructure.Authorization;

/// <summary>
/// Action filter that automatically checks permissions based on controller name and HTTP method.
/// Maps HTTP methods to CRUD operations:
/// - GET -> Read
/// - POST -> Write
/// - PUT/PATCH -> Update
/// - DELETE -> Delete
/// 
/// This filter calls the auth microservice's verify-permission endpoint to check permissions,
/// preventing JWT token bloat from including all permissions.
/// The permission format is: Module-Controller:Action (e.g., SISCON-Accounts:Read)
/// </summary>
public class DynamicPermissionFilter : IAsyncAuthorizationFilter
{
    private readonly Module _module;
    private readonly int _typePermission;

    public DynamicPermissionFilter(Module module = Module.SISCON, int typePermission = 0)
    {
        _module = module;
        _typePermission = typePermission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Skip if user is not authenticated
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get services from DI
        var authClient = context.HttpContext.RequestServices.GetRequiredService<IAuthMicroserviceClient>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<DynamicPermissionFilter>>();

        // Get the bearer token from the request
        var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("No bearer token found in request");
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get controller name
        var controllerName = context.RouteData.Values["controller"]?.ToString() ?? string.Empty;

        // Map HTTP method to permission action
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

        // Log the permission check
        logger.LogDebug("Checking permission: {Module}-{Controller}:{Action} (TypePermission: {TypePermission})", 
            _module, controllerName, action, _typePermission);

        // Call auth microservice to verify permission
        var hasPermission = await authClient.VerifyPermissionAsync(
            token, 
            _module.ToString(), 
            controllerName, 
            action,
            _typePermission
        );

        if (!hasPermission)
        {
            logger.LogWarning("Permission denied for: {Module}-{Controller}:{Action}", 
                _module, controllerName, action);
            context.Result = new ForbidResult();
            return;
        }

        logger.LogInformation("Permission granted for: {Module}-{Controller}:{Action}", 
            _module, controllerName, action);
    }
}

/// <summary>
/// Attribute to apply dynamic permission checking to a controller or action.
/// Use this on controllers to automatically check permissions based on HTTP method.
/// 
/// Default module is SISCON (accountability), but you can specify a different module.
/// TypePermission: 0=ControllerAction (default), 1=MenuOption, 2=UserAction, 3=ProjectView
/// 
/// Usage:
/// [DynamicPermission]                              -> Uses SISCON module, ControllerAction type
/// [DynamicPermission(Module.SISCON)]               -> Explicit SISCON module, ControllerAction type
/// [DynamicPermission(Module.SISCON, 1)]            -> SISCON module, MenuOption type
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class DynamicPermissionAttribute : TypeFilterAttribute
{
    public DynamicPermissionAttribute(Module module = Module.SISCON, int typePermission = 0) 
        : base(typeof(DynamicPermissionFilter))
    {
        Arguments = new object[] { module, typePermission };
    }
}

