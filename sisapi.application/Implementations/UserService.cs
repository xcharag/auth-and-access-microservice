using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sisapi.application.Constants;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.User;
using sisapi.domain.Entities;
using sisapi.infrastructure.Context.Core;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace sisapi.application.Implementations;

public class UserService(
    CoreDbContext context,
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IHttpContextAccessor httpContextAccessor)
    : IUserService
{

    private async Task<string> GetCurrentUsernameAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity is { IsAuthenticated: true } identity)
        {
            var username = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrWhiteSpace(username))
            {
                return username;
            }

            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId)) return "Sistema";
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user != null && !string.IsNullOrWhiteSpace(user.UserName))
            {
                return user.UserName;
            }
        }

        return "Sistema";
    }

    private static bool PasswordMeetsComplexity(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        if (password.Length < 8 || password.Length > 100) return false;
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = Regex.IsMatch(password, "[\\W_]" );
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    public async Task<ApiResponseDto<UserDto>> CreateAsync(CreateUserDto dto)
    {
        try
        {
            if (dto.Roles.Any(IsRestrictedRole))
            {
                return ApiResponseDto<UserDto>.ErrorResponse("Los roles Admin y SuperAdmin no pueden asignarse desde la API");
            }

            if (await userManager.FindByNameAsync(dto.UserName) != null)
            {
                return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.UsernameDuplicado);
            }

            if (await userManager.FindByEmailAsync(dto.Email) != null)
            {
                return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.EmailDuplicado);
            }

            var currentUsername = await GetCurrentUsernameAsync();
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                CompanyId = dto.CompanyId,
                CreatedBy = currentUsername,
                CreatedAt = DateTime.UtcNow,
                Active = true,
                IsDeleted = false
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.CrearUsuarioError, result.Errors.Select(e => e.Description).ToList());
            }

            if (dto.Roles.Count > 0)
            {
                await userManager.AddToRolesAsync(user, dto.Roles);
            }

            var userDto = await MapToDtoAsync(user);
            return ApiResponseDto<UserDto>.SuccessResponse(userDto, UserMessages.UserCreated);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<UserDto>.ErrorResponse($"{ApplicationErrorMessages.CrearUsuarioError}: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<UserDto>> UpdateAsync(int id, UpdateUserDto dto)
    {
        try
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null || user.IsDeleted)
            {
                return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.UsuarioNoEncontrado);
            }

            var wantChangePassword = !string.IsNullOrEmpty(dto.CurrentPassword) || !string.IsNullOrEmpty(dto.NewPassword) || !string.IsNullOrEmpty(dto.ConfirmPassword);

            if (dto.Roles != null && dto.Roles.Any(IsRestrictedRole))
            {
                return ApiResponseDto<UserDto>.ErrorResponse("Los roles Admin y SuperAdmin no pueden asignarse desde la API");
            }

            if (wantChangePassword)
            {
                if (string.IsNullOrEmpty(dto.CurrentPassword) || string.IsNullOrEmpty(dto.NewPassword) || string.IsNullOrEmpty(dto.ConfirmPassword))
                {
                    return ApiResponseDto<UserDto>.ErrorResponse("Para cambiar la contraseña se requieren currentPassword, newPassword y confirmPassword.");
                }

                var currentPwdValid = await userManager.CheckPasswordAsync(user, dto.CurrentPassword);
                if (!currentPwdValid)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse("La contraseña actual no coincide.");
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse("La nueva contraseña y la confirmación no coinciden.");
                }

                if (!PasswordMeetsComplexity(dto.NewPassword))
                {
                    return ApiResponseDto<UserDto>.ErrorResponse("La nueva contraseña debe tener entre 8 y 100 caracteres e incluir mayúscula, minúscula, número y carácter especial.");
                }

                var passwordHasher = userManager.PasswordHasher;
                var verifyResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, dto.NewPassword);
                if (verifyResult == PasswordVerificationResult.Success)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse("La nueva contraseña no puede ser igual a la actual.");
                }

                // Intentar cambiar la contraseña antes de persistir otros cambios.
                // Esto asegura que si el cambio falla por validadores, no se pierde la operación.
                var changeResult = await userManager.ChangePasswordAsync(user, dto.CurrentPassword!, dto.NewPassword!);
                if (!changeResult.Succeeded)
                {
                    // Si el usuario no tiene password (external login), intentar remove/add flow
                    if (!await userManager.HasPasswordAsync(user))
                    {
                        var removeResult = await userManager.RemovePasswordAsync(user);
                        if (!removeResult.Succeeded)
                        {
                            return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.ActualizarUsuarioError, removeResult.Errors.Select(e => e.Description).ToList());
                        }

                        var addResult = await userManager.AddPasswordAsync(user, dto.NewPassword!);
                        if (!addResult.Succeeded)
                        {
                            return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.ActualizarUsuarioError, addResult.Errors.Select(e => e.Description).ToList());
                        }
                    }
                    else
                    {
                        return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.ActualizarUsuarioError, changeResult.Errors.Select(e => e.Description).ToList());
                    }
                }

                // Si llegamos aquí, la contraseña fue cambiada correctamente. Actualizar metadatos.
                user.UpdatedBy = await GetCurrentUsernameAsync();
                user.UpdatedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrEmpty(dto.UserName) && dto.UserName != user.UserName)
            {
                if (await userManager.FindByNameAsync(dto.UserName) != null)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.UsernameDuplicado);
                }
                user.UserName = dto.UserName;
            }

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                if (await userManager.FindByEmailAsync(dto.Email) != null)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.EmailDuplicado);
                }
                user.Email = dto.Email;
            }

            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.CompanyId.HasValue) user.CompanyId = dto.CompanyId.Value;
            if (dto.Active.HasValue) user.Active = dto.Active.Value;

            // Si no se actualizó UpdatedBy/UpdatedAt por el cambio de contraseña, setearlos ahora
            if (string.IsNullOrEmpty(user.UpdatedBy)) user.UpdatedBy = await GetCurrentUsernameAsync();
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.ActualizarUsuarioError, updateResult.Errors.Select(e => e.Description).ToList());
            }


            var userDto = await MapToDtoAsync(user);
            return ApiResponseDto<UserDto>.SuccessResponse(userDto, UserMessages.UserUpdated);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<UserDto>.ErrorResponse($"{ApplicationErrorMessages.ActualizarUsuarioError}: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<bool>> SoftDeleteAsync(int id)
    {
        try
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null || user.IsDeleted)
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.UsuarioNoEncontrado);
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = await GetCurrentUsernameAsync();
            user.Active = false;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.EliminarUsuarioError, result.Errors.Select(e => e.Description).ToList());
            }

            return ApiResponseDto<bool>.SuccessResponse(true, UserMessages.UserDeleted);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<bool>.ErrorResponse($"{ApplicationErrorMessages.EliminarUsuarioError}: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<bool>> RestoreAsync(int id)
    {
        try
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null || !user.IsDeleted)
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.UsuarioNoEliminado);
            }

            user.IsDeleted = false;
            user.DeletedAt = null;
            user.DeletedBy = null;
            user.Active = true;
            user.UpdatedBy = await GetCurrentUsernameAsync();
            user.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.RestaurarUsuarioError, result.Errors.Select(e => e.Description).ToList());
            }

            return ApiResponseDto<bool>.SuccessResponse(true, UserMessages.UserRestored);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<bool>.ErrorResponse($"{ApplicationErrorMessages.RestaurarUsuarioError}: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<UserDto?>> GetByIdAsync(int id)
    {
        try
        {
            var user = await context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
            {
                return ApiResponseDto<UserDto?>.ErrorResponse(ApplicationErrorMessages.UsuarioNoEncontrado);
            }

            var userDto = await MapToDtoAsync(user);
            return ApiResponseDto<UserDto?>.SuccessResponse(userDto, UserMessages.UserRetrieved);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<UserDto?>.ErrorResponse($"{ApplicationErrorMessages.ObtenerUsuarioError}: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<PaginatedResponseDto<UserDto>>> GetAllAsync(UserFilterDto filter)
    {
        try
        {
            var query = context.Users
                .Include(u => u.Company)
                .Where(u => true);

            if (filter.CompanyId.HasValue)
            {
                query = query.Where(u => u.CompanyId == filter.CompanyId.Value);
            }

            if (filter.Active.HasValue)
            {
                query = query.Where(u => u.Active == filter.Active.Value);
            }

            if (filter.IsDeleted.HasValue)
            {
                query = query.Where(u => u.IsDeleted == filter.IsDeleted.Value);
            }
            else
            {
                query = query.Where(u => !u.IsDeleted);
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(u => 
                    u.UserName!.Contains(filter.SearchTerm) ||
                    u.Email!.Contains(filter.SearchTerm) ||
                    (u.FirstName != null && u.FirstName.Contains(filter.SearchTerm)) ||
                    (u.LastName != null && u.LastName.Contains(filter.SearchTerm)));
            }

            if (!string.IsNullOrEmpty(filter.Role))
            {
                var userIdsWithRole = await context.UserRoles
                    .Include(ur => ur.Role)
                    .Where(ur => ur.Role.Name == filter.Role)
                    .Select(ur => ur.UserId)
                    .ToListAsync();
                
                query = query.Where(u => userIdsWithRole.Contains(u.Id));
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "username":
                        query = filter.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName);
                        break;
                    case "email":
                        query = filter.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                        break;
                    case "firstname":
                        query = filter.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName);
                        break;
                    case "lastname":
                        query = filter.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName);
                        break;
                    case "createdat":
                        query = filter.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt);
                        break;
                    default:
                        query = query.OrderBy(u => u.Id);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(u => u.Id);
            }

            var users = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var userDto = await MapToDtoAsync(user);
                userDtos.Add(userDto);
            }

            var paginatedResponse = new PaginatedResponseDto<UserDto>
            {
                Data = userDtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                TotalRecords = totalCount
            };

            return ApiResponseDto<PaginatedResponseDto<UserDto>>.SuccessResponse(paginatedResponse, UserMessages.UsersRetrieved);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<PaginatedResponseDto<UserDto>>.ErrorResponse($"{ApplicationErrorMessages.ObtenerUsuariosError}: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<List<UserDto>>> GetUsersByCompanyAsync(int companyId)
    {
        try
        {
            var users = await context.Users
                .Include(u => u.Company)
                .Where(u => u.CompanyId == companyId && !u.IsDeleted)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var userDto = await MapToDtoAsync(user);
                userDtos.Add(userDto);
            }

            return ApiResponseDto<List<UserDto>>.SuccessResponse(userDtos, UserMessages.UsersRetrieved);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<List<UserDto>>.ErrorResponse($"{ApplicationErrorMessages.ObtenerUsuariosEmpresaError}: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<bool>> AssignRoleAsync(int userId, string roleName)
    {
        try
        {
            if (IsRestrictedRole(roleName))
            {
                return ApiResponseDto<bool>.ErrorResponse("Los roles Admin y SuperAdmin no pueden asignarse desde la API");
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.UsuarioNoEncontrado);
            }

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.RolNoExiste);
            }

            if (await userManager.IsInRoleAsync(user, roleName))
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.UsuarioYaTieneRol);
            }

            var result = await userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.AsignarRolError, result.Errors.Select(e => e.Description).ToList());
            }

            return ApiResponseDto<bool>.SuccessResponse(true, UserMessages.RoleAssigned);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<bool>.ErrorResponse($"{ApplicationErrorMessages.AsignarRolError}: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<bool>> RemoveRoleAsync(int userId, string roleName)
    {
        try
        {
            if (IsRestrictedRole(roleName))
            {
                return ApiResponseDto<bool>.ErrorResponse("Los roles Admin y SuperAdmin no pueden asignarse desde la API");
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.UsuarioNoEncontrado);
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.UsuarioNoTieneRol);
            }

            var result = await userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.RemoverRolError, result.Errors.Select(e => e.Description).ToList());
            }

            return ApiResponseDto<bool>.SuccessResponse(true, UserMessages.RoleRemoved);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<bool>.ErrorResponse($"{ApplicationErrorMessages.RemoverRolError}: {ex.Message}");
        }
    }

    private async Task<UserDto> MapToDtoAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name,
            Active = user.Active,
            IsDeleted = user.IsDeleted,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy,
            Roles = roles.ToList()
        };
    }

    private static bool IsRestrictedRole(string roleName)
    {
        return string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase)
               || string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
    }
}

