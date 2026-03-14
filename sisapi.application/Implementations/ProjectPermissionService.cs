using Microsoft.EntityFrameworkCore;
using sisapi.application.Constants;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Permission;
using sisapi.domain.Entities;
using sisapi.domain.Enum;
using sisapi.infrastructure.Context.Core;

namespace sisapi.application.Implementations;

public class ProjectPermissionService(CoreDbContext context) : IProjectPermissionService
{
    public async Task<ApiResponseDto<PermissionDto>> CreateAsync(CreateProjectPermissionDto dto)
    {
        var projectCode = dto.Code.Trim();

        var existingPermission = await context.Permissions
            .FirstOrDefaultAsync(p => p.Code == projectCode && p.TypePermission == TypePermission.ProjectView);

        if (existingPermission != null)
        {
            return ApiResponseDto<PermissionDto>.ErrorResponse(ApplicationErrorMessages.PermisoYaAsignado);
        }

        var permission = new Permission
        {
            Code = projectCode,
            Module = (Module)dto.Module,
            Description = dto.Description,
            TypePermission = TypePermission.ProjectView,
            CreatedAt = DateTime.UtcNow
        };

        context.Permissions.Add(permission);
        await context.SaveChangesAsync();

        return ApiResponseDto<PermissionDto>.SuccessResponse(new PermissionDto
        {
            Id = permission.Id,
            Code = permission.Code,
            Module = permission.Module.ToString(),
            Description = permission.Description,
            TypePermission = permission.TypePermission.ToString(),
            Active = permission.Active
        }, "Permiso de proyecto creado correctamente");
    }

    public async Task<ApiResponseDto<IEnumerable<PermissionDto>>> GetAllAsync(int? module = null)
    {
        var query = context.Permissions
            .Where(p => p.TypePermission == TypePermission.ProjectView);

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
}