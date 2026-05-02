using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Auth;
using sisapi.infrastructure.Authorization;
using System.Security.Claims;

namespace sisapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [Authorize]
    [DynamicPermission]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await authService.RegisterAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Provision a credential record for another trusted microservice.
    /// </summary>
    [HttpPost("internal/users")]
    [AllowAnonymous]
    public async Task<IActionResult> ProvisionInternalUser([FromBody] InternalProvisionUserRequestDto request)
    {
        if (!IsInternalRequest())
        {
            return Unauthorized(new { Success = false, Message = "Invalid internal API key" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await authService.ProvisionInternalUserAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Mark a credential record as email-confirmed after verification in the owning service.
    /// </summary>
    [HttpPost("internal/users/{userId:int}/confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmInternalUserEmail(int userId)
    {
        if (!IsInternalRequest())
        {
            return Unauthorized(new { Success = false, Message = "Invalid internal API key" });
        }

        var result = await authService.ConfirmEmailInternalAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Deactivate a credential record after a failed cross-service registration.
    /// </summary>
    [HttpDelete("internal/users/{userId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteInternalUser(int userId)
    {
        if (!IsInternalRequest())
        {
            return Unauthorized(new { Success = false, Message = "Invalid internal API key" });
        }

        var result = await authService.DeleteInternalUserAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Login user with username or email and generate JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await authService.LoginAsync(request);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Login endpoint for frontend - sets tokens as HttpOnly cookies
    /// </summary>
    [HttpPost("cookie/login")]
    public async Task<IActionResult> LoginWithCookie([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await authService.LoginAsync(request);
        
        if (!result.Success || result.Data == null)
        {
            return Unauthorized(result);
        }

        Response.Cookies.Append("accessToken", result.Data.Token, BuildCookieOptions(result.Data.ExpiresAt));
        Response.Cookies.Append("refreshToken", result.Data.RefreshToken, BuildCookieOptions(result.Data.RefreshTokenExpiresAt));
 
         // Return success without tokens in body (tokens are in cookies)
         return Ok(new 
         { 
             Success = true,
             result.Message,
             Data = new
             {
                 UserId = result.Data.User.Id,
                 Username = result.Data.User.UserName,
                 result.Data.User.FirstName,
                 result.Data.User.LastName,
                 result.Data.User.Email,
                 result.Data.User.CompanyId,
                 result.Data.User.CompanyName,
                 TokenExpiresAt = result.Data.ExpiresAt,
                 result.Data.RefreshTokenExpiresAt
             }
         });
     }
 
    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await authService.RefreshTokenAsync(request.RefreshToken);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Refresh token endpoint for frontend - reads refresh token from cookie and sets new tokens as cookies
    /// </summary>
    [HttpPost("cookie/refresh-token")]
    public async Task<IActionResult> RefreshTokenWithCookie()
    {
        // Try to get refresh token from cookie
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new { Success = false, Message = "No se encontró el token de actualización" });
        }

        var result = await authService.RefreshTokenAsync(refreshToken);
        
        if (!result.Success || result.Data == null)
        {
            return Unauthorized(result);
        }

        Response.Cookies.Append("accessToken", result.Data.Token, BuildCookieOptions(result.Data.ExpiresAt));
        Response.Cookies.Append("refreshToken", result.Data.RefreshToken, BuildCookieOptions(result.Data.RefreshTokenExpiresAt));
 
         // Return success without tokens in body
         return Ok(new 
         { 
             Success = true, 
             result.Message,
             Data = new
             {
                 TokenExpiresAt = result.Data.ExpiresAt,
                 result.Data.RefreshTokenExpiresAt
             }
         });
     }
 
    /// <summary>
    /// Logout and invalidate current refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        var result = await authService.LogoutAsync(request.RefreshToken);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logout endpoint for frontend - clears cookies and invalidates refresh token
    /// </summary>
    [HttpPost("cookie/logout")]
    [Authorize]
    public async Task<IActionResult> LogoutWithCookie()
    {
        // Try to get refresh token from cookie to revoke it
        if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
        {
            // Revoke the refresh token in the database
            await authService.LogoutAsync(refreshToken);
        }

        var deleteOptions = BuildDeletionCookieOptions();
        Response.Cookies.Delete("accessToken", deleteOptions);
        Response.Cookies.Delete("refreshToken", deleteOptions);
 
         return Ok(new { Success = true, Message = "Sesión cerrada correctamente" });
     }
 
    /// <summary>
    /// Logout from all devices (invalidate all refresh tokens)
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var result = await authService.LogoutAllDevicesAsync(userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logout from all devices for frontend - clears cookies and invalidates all user refresh tokens
    /// </summary>
    [HttpPost("cookie/logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAllWithCookie()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { Success = false, Message = "Usuario no identificado" });
        }

        var result = await authService.LogoutAllDevicesAsync(userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        var deleteOptions = BuildDeletionCookieOptions();
        Response.Cookies.Delete("accessToken", deleteOptions);
        Response.Cookies.Delete("refreshToken", deleteOptions);
 
         return Ok(new { Success = true, Message = "Sesión cerrada en todos los dispositivos" });
     }
 
    /// <summary>
    /// Set a new password for an internal user (called by trusted microservices).
    /// </summary>
    [HttpPost("internal/users/{userId:int}/set-password")]
    [AllowAnonymous]
    public async Task<IActionResult> SetInternalUserPassword(int userId, [FromBody] SetPasswordRequestDto request)
    {
        if (!IsInternalRequest())
        {
            return Unauthorized(new { Success = false, Message = "Invalid internal API key" });
        }

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request?.NewPassword))
        {
            return BadRequest(new { Success = false, Message = "newPassword is required" });
        }

        var result = await authService.SetPasswordInternalAsync(userId, request.NewPassword);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    [HttpPost("assign-role")]
    [Authorize]
    [DynamicPermission]
    public async Task<IActionResult> AssignRole([FromQuery] int userId, [FromQuery] string roleName)
    {
        var result = await authService.AssignRoleToUserAsync(userId, roleName);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [HttpDelete("remove-role")]
    [Authorize]
    [DynamicPermission]
    public async Task<IActionResult> RemoveRole([FromQuery] int userId, [FromQuery] string roleName)
    {
        var result = await authService.RemoveRoleFromUserAsync(userId, roleName);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Soft delete a user
    /// </summary>
    [HttpDelete("user/{userId}")]
    [Authorize]
    [DynamicPermission]
    public async Task<IActionResult> SoftDeleteUser(int userId)
    {
        var result = await authService.SoftDeleteUserAsync(userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Restore a soft deleted user
    /// </summary>
    [HttpPost("user/{userId}/restore")]
    [Authorize]
    [DynamicPermission]
    public async Task<IActionResult> RestoreUser(int userId)
    {
        var result = await authService.RestoreUserAsync(userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Debug endpoint to check user claims and permissions (no dynamic permission check)
    /// </summary>
    [HttpGet("debug/claims")]
    [Authorize]
    public IActionResult GetUserClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var permissions = User.Claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
        
        return Ok(new
        {
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            UserName = User.FindFirst(ClaimTypes.Name)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
            AllClaims = claims,
            Permissions = permissions,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        });
    }

    /// <summary>
    /// Verify if the current user has permission for a specific module, controller, and action.
    /// Used by other microservices to check permissions without embedding them in JWT.
    /// </summary>
    /// <param name="module">The module name (e.g., SISAPI, SIGA, SISCON)</param>
    /// <param name="controller">The controller name (e.g., User, Company)</param>
    /// <param name="action">The action type (Read, Write, Update, Delete)</param>
    /// <param name="typePermission"></param>
    [HttpGet("verify-permission")]
    [Authorize]
    public async Task<IActionResult> VerifyPermission(
        [FromQuery] string module, 
        [FromQuery] string controller, 
        [FromQuery] string action,
        [FromQuery] int typePermission = 0)
    {
        if (string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action))
        {
            return BadRequest(new { Success = false, Message = "Módulo, controlador y acción son requeridos" });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { Success = false, Message = "Usuario no identificado" });
        }

        var result = await authService.VerifyPermissionAsync(userId, module, controller, action, typePermission);
        
        return Ok(result);
    }

    private CookieOptions BuildCookieOptions(DateTime expiresAt)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt,
            Path = "/"
        };

        var domain = ResolveCookieDomain();
        if (!string.IsNullOrWhiteSpace(domain))
        {
            options.Domain = domain;
        }

        return options;
    }

    private CookieOptions BuildDeletionCookieOptions()
    {
        var options = new CookieOptions
        {
            Path = "/",
            Secure = true,
            SameSite = SameSiteMode.None
        };

        var domain = ResolveCookieDomain();
        if (!string.IsNullOrWhiteSpace(domain))
        {
            options.Domain = domain;
        }

        return options;
    }

    private string? ResolveCookieDomain()
    {
        var configuredDomain = configuration["CookieDomain"];
        if (string.IsNullOrWhiteSpace(configuredDomain))
        {
            return null;
        }

        var host = HttpContext.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(host))
        {
            return configuredDomain;
        }

        if (host.Contains("localhost", StringComparison.OrdinalIgnoreCase) || host.StartsWith("127.", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return configuredDomain;
    }

    private bool IsInternalRequest()
    {
        var configuredKey = configuration["InternalApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            return false;
        }

        var suppliedKey = Request.Headers["X-Internal-Api-Key"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(suppliedKey)
               && string.Equals(suppliedKey, configuredKey, StringComparison.Ordinal);
    }
 }
