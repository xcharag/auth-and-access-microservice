using System.Linq;
using Microsoft.EntityFrameworkCore;
using sisapi.application.Constants;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.RolePermission;
using sisapi.domain.Entities;
using sisapi.domain.Enum;
using sisapi.infrastructure.Context.Core;

namespace sisapi.application.Implementations;

public class RolePermissionService(CoreDbContext context) : IRolePermissionService
{
    private static readonly DateTime ExpirationSentinel = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    // Whitelist used by the optional onlyAccounting filter
    private static readonly string[] AccountingPermissionCodes = new[] { "Cuenta_Ajustes_Por_Inflacion", "Cuenta_Diferencia_Cambio" };

    private async Task<Role?> GetCompanyRoleAsync(int roleId, int companyId)
    {
        return await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.CompanyId == companyId && r.Active);
    }

    public async Task<ApiResponseDto<RolePermissionDto>> AssignPermissionToRoleAsync(AssignPermissionToRoleDto dto, int companyId)
    {
        return await CreateRolePermissionAsync(dto, companyId, "Permiso asignado correctamente");
    }

    public async Task<ApiResponseDto<bool>> RemovePermissionFromRoleAsync(int roleId, int permissionId, int companyId)
    {
        var rolePermission = await context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && (rp.CompanyId == companyId || rp.CompanyId == null) && rp.Active);

        if (rolePermission == null)
        {
            return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.PermisoRolNoEncontrado);
        }

        rolePermission.Active = false;
        rolePermission.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(true, "Permiso removido correctamente");
    }

    public async Task<ApiResponseDto<List<RolePermissionDto>>> GetRolePermissionsAsync(int roleId, int companyId)
    {
        bool hasCompany = companyId > 0;

        // Resolve role: if company provided, ensure role belongs to company; otherwise accept any role with that id
        Role? role = null;
        if (hasCompany)
        {
            role = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.CompanyId == companyId && r.Active);
            if (role == null)
            {
                var msg = $"El rol {roleId} no pertenece a la compañía {companyId}";
                return ApiResponseDto<List<RolePermissionDto>>.ErrorResponse(msg);
            }
        }
        else
        {
            role = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.Active);
            if (role == null)
            {
                return ApiResponseDto<List<RolePermissionDto>>.ErrorResponse(ApplicationErrorMessages.RolNoExiste);
            }
        }

        // Build query: if no company filter, return all RolePermissions for the role; otherwise include company-specific and global
        var rolePermissionsQuery = context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId && rp.Active);

        if (hasCompany)
        {
            rolePermissionsQuery = rolePermissionsQuery.Where(rp => rp.CompanyId == companyId || rp.CompanyId == null);
        }

        var rolePermissions = await rolePermissionsQuery.ToListAsync();

        var rolePermissionDtos = rolePermissions.Select(rp => new RolePermissionDto
        {
            Id = rp.Id,
            RoleId = rp.RoleId,
            RoleName = role.Name ?? string.Empty,
            PermissionId = rp.PermissionId,
            PermissionCode = rp.Permission.Code,
            Read = rp.Read,
            Write = rp.Write,
            Update = rp.Update,
            Delete = rp.Delete,
            ExpirationDate = rp.ExpirationDate,
            Active = rp.Active,
            CompanyId = rp.CompanyId
        }).ToList();

        return ApiResponseDto<List<RolePermissionDto>>.SuccessResponse(rolePermissionDtos);
    }

    public async Task<ApiResponseDto<List<RolePermissionDto>>> GetRolePermissionsFilteredAsync(int roleId, int companyId, int? module, int? typePermission, bool onlyAccounting = false)
    {
        bool hasCompany = companyId > 0;

        // Resolve role according to whether company filter is provided
        Role? role = null;
        if (hasCompany)
        {
            role = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.CompanyId == companyId && r.Active);
            if (role == null)
            {
                var msg = $"El rol {roleId} no pertenece a la compañía {companyId}";
                return ApiResponseDto<List<RolePermissionDto>>.ErrorResponse(msg);
            }
        }
        else
        {
            role = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.Active);
            if (role == null)
            {
                return ApiResponseDto<List<RolePermissionDto>>.ErrorResponse(ApplicationErrorMessages.RolNoExiste);
            }
        }

        var query = context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId && rp.Active);

        if (hasCompany)
        {
            // Apply company/type rules: for ProjectView require exact company match; for others allow company-specific or global
            query = query.Where(rp => (rp.Permission.TypePermission == TypePermission.ProjectView && rp.CompanyId == companyId)
                                       || (rp.Permission.TypePermission != TypePermission.ProjectView && (rp.CompanyId == companyId || rp.CompanyId == null)));
        }
        else
        {
            // No company filter: return permissions regardless of CompanyId. Only apply type/module filters below.
        }

        // Apply optional onlyAccounting whitelist filter
        if (onlyAccounting)
        {
            query = query.Where(rp => AccountingPermissionCodes.Contains(rp.Permission.Code));
        }

        if (module.HasValue)
        {
            var moduleEnum = (Module)module.Value;
            query = query.Where(rp => rp.Permission.Module == moduleEnum);
        }

        if (typePermission.HasValue)
        {
            var typePermissionEnum = (TypePermission)typePermission.Value;
            query = query.Where(rp => rp.Permission.TypePermission == typePermissionEnum);
        }

        var rolePermissions = await query.ToListAsync();

        var rolePermissionDtos = rolePermissions.Select(rp => new RolePermissionDto
        {
            Id = rp.Id,
            RoleId = rp.RoleId,
            RoleName = role.Name ?? string.Empty,
            PermissionId = rp.PermissionId,
            PermissionCode = rp.Permission.Code,
            Read = rp.Read,
            Write = rp.Write,
            Update = rp.Update,
            Delete = rp.Delete,
            ExpirationDate = rp.ExpirationDate,
            Active = rp.Active,
            CompanyId = rp.CompanyId
        }).ToList();

        return ApiResponseDto<List<RolePermissionDto>>.SuccessResponse(rolePermissionDtos);
    }

    public async Task<ApiResponseDto<RolePermissionDto>> UpdateRolePermissionAsync(int id, AssignPermissionToRoleDto dto, int companyId)
    {
        // Obtener role sin filtrar por companyId para poder resolver la compañía asignada
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId && r.Active);
        if (role == null)
        {
            return ApiResponseDto<RolePermissionDto>.ErrorResponse(ApplicationErrorMessages.RolNoExiste);
        }

        // Resolver companyId definitivo: preferir la company del role si existe, sino usar el companyId pasado por query
        int? assignedCompanyId = role.CompanyId ?? (companyId > 0 ? companyId : (int?)null);
        if (!assignedCompanyId.HasValue)
        {
            return ApiResponseDto<RolePermissionDto>.ErrorResponse("CompanyId es requerido para actualizar/crear este permiso");
        }

        var permission = await context.Permissions
            .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
            .FirstOrDefaultAsync(p => p.Id == dto.PermissionId);
        if (permission == null)
        {
            return ApiResponseDto<RolePermissionDto>.ErrorResponse(ApplicationErrorMessages.PermisoNoEncontrado);
        }

        // Validar que el permiso no pertenezca a otra compañía
        if (permission.RolePermissions.Any() && !permission.RolePermissions.Any(rp => rp.CompanyId == assignedCompanyId.Value || rp.CompanyId == null))
        {
            return ApiResponseDto<RolePermissionDto>.ErrorResponse("El permiso pertenece a otra compañía");
        }

        var rolePermissionById = await context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.Id == id && (rp.CompanyId == assignedCompanyId.Value || rp.CompanyId == null));

        if (rolePermissionById != null && rolePermissionById.RoleId == dto.RoleId && rolePermissionById.PermissionId == dto.PermissionId)
        {
            ApplyRolePermissionUpdates(rolePermissionById, dto, assignedCompanyId.Value);
            await context.SaveChangesAsync();

            return ApiResponseDto<RolePermissionDto>.SuccessResponse(BuildRolePermissionDto(rolePermissionById), "Permiso actualizado correctamente");
        }

        var targetRolePermission = await context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId && (rp.CompanyId == assignedCompanyId.Value || rp.CompanyId == null) && rp.Active);

        if (targetRolePermission != null)
        {
            ApplyRolePermissionUpdates(targetRolePermission, dto, assignedCompanyId.Value);
            await context.SaveChangesAsync();

            return ApiResponseDto<RolePermissionDto>.SuccessResponse(BuildRolePermissionDto(targetRolePermission), "Permiso actualizado correctamente");
        }

        // No existe: crear uno nuevo usando assignedCompanyId
        return await CreateRolePermissionAsync(dto, assignedCompanyId.Value, "Permiso asignado correctamente", validateExisting: false);
    }

    private static void ApplyRolePermissionUpdates(RolePermission rolePermission, AssignPermissionToRoleDto dto, int assignedCompanyId)
    {
        rolePermission.CompanyId = assignedCompanyId;
        rolePermission.Read = dto.Read;
        rolePermission.Write = dto.Write;
        rolePermission.Update = dto.Update;
        rolePermission.Delete = dto.Delete;
        rolePermission.ExpirationDate = NormalizeExpiration(dto.ExpirationDate);
        rolePermission.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<ApiResponseDto<RolePermissionDto>> CreateRolePermissionAsync(AssignPermissionToRoleDto dto, int companyId, string successMessage, bool validateExisting = true)
    {
        // Obtener role sin filtrar por companyId: necesitamos saber si el role ya está asociado a una compañía
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId && r.Active);
        if (role == null)
        {
            return ApiResponseDto<RolePermissionDto>.ErrorResponse(ApplicationErrorMessages.RolNoExiste);
        }

        var permission = await context.Permissions
            .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
            .FirstOrDefaultAsync(p => p.Id == dto.PermissionId);
        if (permission == null)
        {
            return ApiResponseDto<RolePermissionDto>.ErrorResponse(ApplicationErrorMessages.PermisoNoEncontrado);
        }

        // Resolver companyId definitivo: preferir la company del role si existe, sino usar el companyId pasado por query
        int? assignedCompanyId = role.CompanyId ?? (companyId > 0 ? companyId : (int?)null);
        if (!assignedCompanyId.HasValue)
        {
            return ApiResponseDto<RolePermissionDto>.ErrorResponse("CompanyId es requerido para asignar este permiso al role");
        }

        // Si el permiso tiene RolePermissions pero ninguna corresponde a esta compañía ni es global, entonces pertenece a otra compañía
        if (permission.RolePermissions.Any() && !permission.RolePermissions.Any(rp => rp.CompanyId == assignedCompanyId.Value || rp.CompanyId == null))
        {
            return ApiResponseDto<RolePermissionDto>.ErrorResponse("El permiso pertenece a otra compañía");
        }

        if (validateExisting)
        {
            // For ControllerAction permissions we must ensure uniqueness globally (no duplicates across companies)
            if (permission.TypePermission == TypePermission.ControllerAction)
            {
                var existingGlobal = await context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId && rp.Active);

                if (existingGlobal != null)
                {
                    return ApiResponseDto<RolePermissionDto>.ErrorResponse(ApplicationErrorMessages.PermisoYaAsignado);
                }
            }
            else
            {
                // For MenuOption, ProjectView and UserAction allow same Role+Permission across different companies,
                // but prevent duplicates within the same company (and global duplicates). The DB unique index on (CompanyId, RoleId, PermissionId)
                // will prevent duplicates within a company; here just check for existing active in this company or as global.
                var existingRolePermission = await context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId && (rp.CompanyId == assignedCompanyId.Value || rp.CompanyId == null) && rp.Active);

                if (existingRolePermission != null)
                {
                    return ApiResponseDto<RolePermissionDto>.ErrorResponse(ApplicationErrorMessages.PermisoYaAsignado);
                }
            }
        }

        var rolePermission = new RolePermission
        {
            RoleId = dto.RoleId,
            PermissionId = dto.PermissionId,
            Read = dto.Read,
            Write = dto.Write,
            Update = dto.Update,
            Delete = dto.Delete,
            ExpirationDate = NormalizeExpiration(dto.ExpirationDate),
            CompanyId = assignedCompanyId.Value,
            CreatedAt = DateTime.UtcNow
        };

        context.RolePermissions.Add(rolePermission);
        await context.SaveChangesAsync();

        rolePermission.Role = role;
        rolePermission.Permission = permission;

        return ApiResponseDto<RolePermissionDto>.SuccessResponse(BuildRolePermissionDto(rolePermission), successMessage);
    }

    private static RolePermissionDto BuildRolePermissionDto(RolePermission rolePermission)
    {
        return new RolePermissionDto
        {
            Id = rolePermission.Id,
            RoleId = rolePermission.RoleId,
            RoleName = rolePermission.Role?.Name ?? string.Empty,
            PermissionId = rolePermission.PermissionId,
            PermissionCode = rolePermission.Permission?.Code ?? string.Empty,
            Read = rolePermission.Read,
            Write = rolePermission.Write,
            Update = rolePermission.Update,
            Delete = rolePermission.Delete,
            ExpirationDate = rolePermission.ExpirationDate,
            Active = rolePermission.Active,
            CompanyId = rolePermission.CompanyId
        };
    }

    private static DateTime? NormalizeExpiration(DateTime? expirationDate)
    {
        if (!expirationDate.HasValue || expirationDate.Value == default)
        {
            return null;
        }

        var value = expirationDate.Value;
        if (value.Kind == DateTimeKind.Unspecified)
        {
            return value;
        }

        var universalValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return DateTime.SpecifyKind(universalValue, DateTimeKind.Unspecified);
    }

}
