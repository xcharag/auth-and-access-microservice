using Microsoft.AspNetCore.Identity;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Auth;
using sisapi.domain.Dtos.Common;
using sisapi.domain.Entities;
using sisapi.application.Constants;
using sisapi.infrastructure.Context.Core;


namespace sisapi.application.Implementations;

public class AuthService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IJwtService jwtService,
    CoreDbContext context)
    : IAuthService
{
    public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Email already exists");
        }

        var existingUsername = await userManager.FindByNameAsync(request.UserName);
        if (existingUsername != null)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Username already exists");
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            CompanyId = request.CompanyId,
            CreatedAt = DateTime.UtcNow,
            Active = true,
            IsDeleted = false
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse(
                "Registration failed",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        var (token, tokenExpiresAt) = await jwtService.GenerateTokenAsync(user);
        var (refreshToken, refreshTokenExpiresAt) = await jwtService.GenerateRefreshTokenAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        var permissions = await jwtService.GetUserPermissionsAsync(user);

        var authResponse = new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = tokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };

        return ApiResponseDto<AuthResponseDto>.SuccessResponse(authResponse, "User registered successfully");
    }

    public async Task<ApiResponseDto<AuthResponseDto>> ProvisionInternalUserAsync(InternalProvisionUserRequestDto request)
    {
        var normalizedEmail = request.Email.Trim();
        var userName = string.IsNullOrWhiteSpace(request.UserName)
            ? normalizedEmail
            : request.UserName.Trim();

        var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Email already exists");
        }

        var existingUsername = await userManager.FindByNameAsync(userName);
        if (existingUsername != null)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Username already exists");
        }

        var user = new User
        {
            UserName = userName,
            Email = normalizedEmail,
            EmailConfirmed = request.EmailConfirmed,
            CreatedAt = DateTime.UtcNow,
            Active = true,
            IsDeleted = false
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse(
                "Registration failed",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        var roleName = string.IsNullOrWhiteSpace(request.RoleName) ? "NibuAppUser" : request.RoleName.Trim();
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new Role
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = "Default mobile app user role",
                Active = true,
                CreatedAt = DateTime.UtcNow
            });

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return ApiResponseDto<AuthResponseDto>.ErrorResponse(
                    "Role creation failed",
                    roleResult.Errors.Select(e => e.Description).ToList()
                );
            }
        }

        var assignRoleResult = await userManager.AddToRoleAsync(user, roleName);
        if (!assignRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return ApiResponseDto<AuthResponseDto>.ErrorResponse(
                "Role assignment failed",
                assignRoleResult.Errors.Select(e => e.Description).ToList()
            );
        }

        var authResponse = await BuildAuthResponseAsync(user);
        return ApiResponseDto<AuthResponseDto>.SuccessResponse(authResponse, "User provisioned successfully");
    }

    public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await userManager.FindByEmailAsync(request.UsernameOrEmail)
                   ?? await userManager.FindByNameAsync(request.UsernameOrEmail);

        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Usuario/Email o Contraseña incorrectos");
        }

        if (!user.Active)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("La cuenta de usuario no está activada");
        }

        if (user.IsDeleted)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("La cuenta de usuario ha sido eliminada");
        }

        if (user.CompanyId.HasValue)
        {
            await context.Entry(user).Reference(u => u.Company).LoadAsync();
        }

        var (token, tokenExpiresAt) = await jwtService.GenerateTokenAsync(user);
        var (refreshToken, refreshTokenExpiresAt) = await jwtService.GenerateRefreshTokenAsync(user, request.RememberMe);
        var roles = await userManager.GetRolesAsync(user);
        var permissions = await jwtService.GetUserPermissionsAsync(user);

        var authResponse = new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = tokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };

        return ApiResponseDto<AuthResponseDto>.SuccessResponse(authResponse, "Se ha iniciado sesión correctamente");
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var isValid = await jwtService.ValidateRefreshTokenAsync(refreshToken);
        if (!isValid)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Token de actualización no válido o expirado");
        }

        var storedToken = await jwtService.GetRefreshTokenAsync(refreshToken);
        if (storedToken == null)
        {
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("Token de actualización no encontrado");
        }

        var user = storedToken.User;
        if (!user.Active || user.IsDeleted)
        {
            await jwtService.RevokeRefreshTokenAsync(refreshToken);
            return ApiResponseDto<AuthResponseDto>.ErrorResponse("La cuenta de usuario no está activa o ha sido eliminada");
        }

        var daysUntilExpiry = (storedToken.ExpiresAt - storedToken.CreatedAt).TotalDays;
        var isRememberMe = daysUntilExpiry > 30;

        var (newToken, tokenExpiresAt) = await jwtService.GenerateTokenAsync(user);
        var (newRefreshToken, refreshTokenExpiresAt) = await jwtService.GenerateRefreshTokenAsync(user, isRememberMe);

        await jwtService.RevokeRefreshTokenAsync(refreshToken, newRefreshToken);

        var roles = await userManager.GetRolesAsync(user);
        var permissions = await jwtService.GetUserPermissionsAsync(user);

        var authResponse = new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = tokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };

        return ApiResponseDto<AuthResponseDto>.SuccessResponse(authResponse, "Token actualizado correctamente");
    }

    public async Task<ApiResponseDto<bool>> LogoutAsync(string refreshToken)
    {
        var storedToken = await jwtService.GetRefreshTokenAsync(refreshToken);
        if (storedToken == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Token de actualización no encontrado");
        }

        await jwtService.RevokeRefreshTokenAsync(refreshToken);
        return ApiResponseDto<bool>.SuccessResponse(true, "Cierre de sesión exitoso, hasta luego");
    }

    public async Task<ApiResponseDto<bool>> LogoutAllDevicesAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Usuario no encontrado");
        }

        await jwtService.RevokeAllUserRefreshTokensAsync(userId);
        return ApiResponseDto<bool>.SuccessResponse(true, "Cierre de sesión en todos los dispositivos exitoso, hasta luego");
    }

    public async Task<ApiResponseDto<bool>> AssignRoleToUserAsync(int userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Usuario no encontrado");
        }

        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            return ApiResponseDto<bool>.ErrorResponse("Rol no encontrado");
        }

        var result = await userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "Fallo al asignar el rol",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        return ApiResponseDto<bool>.SuccessResponse(true, "Rol asignado exitosamente");
    }

    public async Task<ApiResponseDto<bool>> RemoveRoleFromUserAsync(int userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Usuario no encontrado");
        }

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "Failed to remove role",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        return ApiResponseDto<bool>.SuccessResponse(true, "Rol removido exitosamente");
    }

    public async Task<ApiResponseDto<bool>> SoftDeleteUserAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Usuario no encontrado");
        }

        if (user.IsDeleted)
        {
            return ApiResponseDto<bool>.ErrorResponse("El usuario ya ha sido eliminado");
        }

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.Active = false;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "Fallo al eliminar el usuario",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        await jwtService.RevokeAllUserRefreshTokensAsync(userId);

        return ApiResponseDto<bool>.SuccessResponse(true, AuthMessages.UserDeleted);
    }

    public async Task<ApiResponseDto<bool>> RestoreUserAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.ErrorResponse(AuthMessages.UserNotFound);
        }

        if (!user.IsDeleted)
        {
            return ApiResponseDto<bool>.ErrorResponse(AuthMessages.UserNotDeleted);
        }

        user.IsDeleted = false;
        user.DeletedAt = null;
        user.DeletedBy = null;
        user.Active = true;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                AuthMessages.UpdateUserFailed,
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        return ApiResponseDto<bool>.SuccessResponse(true, AuthMessages.UserRestored);
    }

    public async Task<ApiResponseDto<bool>> ConfirmEmailInternalAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.ErrorResponse(AuthMessages.UserNotFound);
        }

        user.EmailConfirmed = true;
        user.Active = true;
        user.IsDeleted = false;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? ApiResponseDto<bool>.SuccessResponse(true, "Email confirmed successfully")
            : ApiResponseDto<bool>.ErrorResponse("Email confirmation failed", result.Errors.Select(e => e.Description).ToList());
    }

    public async Task<ApiResponseDto<bool>> DeleteInternalUserAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.SuccessResponse(true, "User already absent");
        }

        user.IsDeleted = true;
        user.Active = false;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponseDto<bool>.ErrorResponse("User rollback failed", result.Errors.Select(e => e.Description).ToList());
        }

        await jwtService.RevokeAllUserRefreshTokensAsync(userId);
        return ApiResponseDto<bool>.SuccessResponse(true, "User deactivated successfully");
    }

    public async Task<ApiResponseDto<bool>> VerifyPermissionAsync(int userId, string module, string controller, string action, int typePermission = 0)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.ErrorResponse(AuthMessages.UserNotFound);
        }

        if (!user.Active || user.IsDeleted)
        {
            return ApiResponseDto<bool>.ErrorResponse(AuthMessages.UserNotActive);
        }

        var requiredPermission = $"{module}-{controller}:{action}";

        var permissions = await jwtService.GetUserPermissionsAsync(user, typePermission);

        var hasPermission = permissions.Contains(requiredPermission);

        if (hasPermission)
        {
            return ApiResponseDto<bool>.SuccessResponse(true, AuthMessages.PermissionGranted);
        }

        return ApiResponseDto<bool>.ErrorResponse(AuthMessages.PermissionDenied);
    }

    public async Task<ApiResponseDto<bool>> SetPasswordInternalAsync(int userId, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "Usuario no encontrado",
                new List<string> { $"El userId {userId} no existe en el sistema" }
            );
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!result.Succeeded)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "No se pudo actualizar la contraseña",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        // Invalidate all existing refresh tokens so prior sessions become invalid
        await jwtService.RevokeAllUserRefreshTokensAsync(userId);

        return ApiResponseDto<bool>.SuccessResponse(true, "Contraseña actualizada correctamente");
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user, bool rememberMe = false)
    {
        var (token, tokenExpiresAt) = await jwtService.GenerateTokenAsync(user);
        var (refreshToken, refreshTokenExpiresAt) = await jwtService.GenerateRefreshTokenAsync(user, rememberMe);
        var roles = await userManager.GetRolesAsync(user);
        var permissions = await jwtService.GetUserPermissionsAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = tokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };
    }
}
