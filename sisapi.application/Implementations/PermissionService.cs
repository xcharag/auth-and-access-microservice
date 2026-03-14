using Microsoft.EntityFrameworkCore;
using sisapi.application.Constants;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Permission;
using sisapi.domain.Entities;
using sisapi.infrastructure.Context.Core;

namespace sisapi.application.Implementations;

public class PermissionService(CoreDbContext context) : IPermissionService
{
    public async Task<ApiResponseDto<PermissionDto>> CreateAsync(CreatePermissionDto dto)
    {
        var existingPermission = await context.Permissions
            .FirstOrDefaultAsync(p => p.Code == dto.Code);

        if (existingPermission != null)
        {
            return ApiResponseDto<PermissionDto>.ErrorResponse("Permission with this code already exists");
        }

        var permission = new Permission
        {
            Code = dto.Code,
            Module = (domain.Enum.Module)dto.Module,
            Description = dto.Description,
            TypePermission = (domain.Enum.TypePermission)dto.TypePermission,
            CreatedAt = DateTime.UtcNow
        };

        context.Permissions.Add(permission);
        await context.SaveChangesAsync();

        var permissionDto = new PermissionDto
        {
            Id = permission.Id,
            Code = permission.Code,
            Module = permission.Module.ToString(),
            Description = permission.Description,
            TypePermission = permission.TypePermission.ToString(),
            Active = permission.Active
        };

        return ApiResponseDto<PermissionDto>.SuccessResponse(permissionDto, "Permission created successfully");
    }

    public async Task<ApiResponseDto<PermissionDto>> UpdateAsync(int id, UpdatePermissionDto dto)
    {
        var permission = await context.Permissions.FindAsync(id);
        if (permission == null)
        {
            return ApiResponseDto<PermissionDto>.ErrorResponse("Permission not found");
        }

        permission.Code = dto.Code;
        permission.Module = (domain.Enum.Module)dto.Module;
        permission.Description = dto.Description;
        permission.TypePermission = (domain.Enum.TypePermission)dto.TypePermission;
        permission.Active = dto.Active;
        permission.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var permissionDto = new PermissionDto
        {
            Id = permission.Id,
            Code = permission.Code,
            Module = permission.Module.ToString(),
            Description = permission.Description,
            TypePermission = permission.TypePermission.ToString(),
            Active = permission.Active
        };

        return ApiResponseDto<PermissionDto>.SuccessResponse(permissionDto, "Permission updated successfully");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        var permission = await context.Permissions.FindAsync(id);
        if (permission == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Permission not found");
        }

        permission.Active = false;
        permission.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(true, "Permission deleted successfully");
    }

    public async Task<ApiResponseDto<PermissionDto?>> GetByIdAsync(int id)
    {
        var permission = await context.Permissions.FindAsync(id);
        if (permission == null)
        {
            return ApiResponseDto<PermissionDto?>.ErrorResponse("Permission not found");
        }

        var permissionDto = new PermissionDto
        {
            Id = permission.Id,
            Code = permission.Code,
            Module = permission.Module.ToString(),
            Description = permission.Description,
            TypePermission = permission.TypePermission.ToString(),
            Active = permission.Active
        };

        return ApiResponseDto<PermissionDto?>.SuccessResponse(permissionDto);
    }

    public async Task<ApiResponseDto<PaginatedResponseDto<PermissionDto>>> GetAllAsync(PermissionFilterDto filter)
    {
        var query = context.Permissions.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Code))
        {
            query = query.Where(p => p.Code.Contains(filter.Code));
        }

        if (filter.Module.HasValue)
        {
            query = query.Where(p => (int)p.Module == filter.Module.Value);
        }

        if (filter.TypePermission.HasValue)
        {
            query = query.Where(p => (int)p.TypePermission == filter.TypePermission.Value);
        }

        if (filter.Active.HasValue)
        {
            query = query.Where(p => p.Active == filter.Active.Value);
        }

        // Get total count
        var totalRecords = await query.CountAsync();

        // Apply sorting
        query = !string.IsNullOrWhiteSpace(filter.SortBy)
            ? filter.SortBy.ToLower() switch
            {
                "code" => filter.SortDescending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
                "module" => filter.SortDescending ? query.OrderByDescending(p => p.Module) : query.OrderBy(p => p.Module),
                "createdat" => filter.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Id)
            }
            : query.OrderBy(p => p.Id);

        // Apply pagination
        var permissions = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Code = p.Code,
            Module = p.Module.ToString(),
            Description = p.Description,
            TypePermission = p.TypePermission.ToString(),
            Active = p.Active
        }).ToList();

        var paginatedResponse = new PaginatedResponseDto<PermissionDto>
        {
            Data = permissionDtos,
            TotalRecords = totalRecords,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponseDto<PaginatedResponseDto<PermissionDto>>.SuccessResponse(paginatedResponse);
    }

    public async Task<ApiResponseDto<List<PermissionDto>>> GetByModuleAsync(int module)
    {
        var permissions = await context.Permissions
            .Where(p => (int)p.Module == module && p.Active)
            .ToListAsync();

        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Code = p.Code,
            Module = p.Module.ToString(),
            Description = p.Description,
            TypePermission = p.TypePermission.ToString(),
            Active = p.Active
        }).ToList();

        return ApiResponseDto<List<PermissionDto>>.SuccessResponse(permissionDtos);
    }

    public async Task<ApiResponseDto<List<UserPermissionDto>>> GetUserPermissionsAsync(int userId, int? module, int? typePermission)
    {
        var user = await context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.Active);

        if (user == null)
        {
            return ApiResponseDto<List<UserPermissionDto>>.ErrorResponse(ApplicationErrorMessages.UsuarioNoEncontrado);
        }

        var rolePermissionsQuery = context.RolePermissions
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => rp.Active && rp.Role.Active && rp.Role.UserRoles.Any(ur => ur.UserId == userId) && (!rp.ExpirationDate.HasValue || rp.ExpirationDate >= DateTime.UtcNow));

        if (module.HasValue)
        {
            var moduleEnum = (domain.Enum.Module)module.Value;
            rolePermissionsQuery = rolePermissionsQuery.Where(rp => rp.Permission.Module == moduleEnum);
        }

        if (typePermission.HasValue)
        {
            var typeEnum = (domain.Enum.TypePermission)typePermission.Value;
            rolePermissionsQuery = rolePermissionsQuery.Where(rp => rp.Permission.TypePermission == typeEnum);
        }

        var rolePermissions = await rolePermissionsQuery.ToListAsync();

        var groupedPermissions = rolePermissions
            .GroupBy(rp => new { rp.RoleId, rp.PermissionId })
            .Select(g => g.First())
            .ToList();

        var userPermissions = groupedPermissions.Select(rp => new UserPermissionDto
        {
            RoleId = rp.RoleId,
            RoleName = rp.Role.Name ?? string.Empty,
            PermissionId = rp.PermissionId,
            PermissionCode = rp.Permission.Code,
            Module = rp.Permission.Module.ToString(),
            Description = rp.Permission.Description,
            TypePermission = rp.Permission.TypePermission.ToString(),
            Read = rp.Read,
            Write = rp.Write,
            Update = rp.Update,
            Delete = rp.Delete,
            ExpirationDate = rp.ExpirationDate
        }).ToList();

        return ApiResponseDto<List<UserPermissionDto>>.SuccessResponse(userPermissions);
    }
}
