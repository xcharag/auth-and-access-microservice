using Microsoft.EntityFrameworkCore;
using sisapi.application.Constants;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Permission;
using sisapi.domain.Entities;
using sisapi.domain.Enum;
using sisapi.infrastructure.Context.Core;

namespace sisapi.application.Implementations;

public class MenuPermissionService(CoreDbContext context) : IMenuPermissionService
{
    public async Task<ApiResponseDto<PermissionDto>> CreateAsync(CreateMenuPermissionDto dto)
    {
        var code = dto.Code.Trim();

        var exists = await context.Permissions
            .AnyAsync(p => p.Code == code && p.TypePermission == TypePermission.UserAction);

        if (exists)
        {
            return ApiResponseDto<PermissionDto>.ErrorResponse(ApplicationErrorMessages.PermisoYaAsignado);
        }

        var permission = new Permission
        {
            Code = code,
            Module = (Module)dto.Module,
            Description = dto.Description,
            TypePermission = TypePermission.UserAction,
            CreatedAt = DateTime.UtcNow
        };

        context.Permissions.Add(permission);
        await context.SaveChangesAsync();

        return ApiResponseDto<PermissionDto>.SuccessResponse(MapToDto(permission), "Permiso de menú creado correctamente");
    }

    public async Task<ApiResponseDto<IEnumerable<PermissionDto>>> GetAllAsync(int? module = null)
    {
        var query = context.Permissions
            .Where(p => p.TypePermission == TypePermission.UserAction);

        if (module.HasValue)
        {
            var moduleEnum = (Module)module.Value;
            query = query.Where(p => p.Module == moduleEnum);
        }

        var permissions = await query
            .OrderBy(p => p.Code)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Module = p.Module.ToString(),
                Description = p.Description,
                TypePermission = p.TypePermission.ToString(),
                Active = p.Active
            })
            .ToListAsync();

        return ApiResponseDto<IEnumerable<PermissionDto>>.SuccessResponse(permissions);
    }

    private static PermissionDto MapToDto(Permission permission) => new()
    {
        Id = permission.Id,
        Code = permission.Code,
        Module = permission.Module.ToString(),
        Description = permission.Description,
        TypePermission = permission.TypePermission.ToString(),
        Active = permission.Active
    };
}

