using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Role;
using sisapi.domain.Entities;
using sisapi.infrastructure.Context.Core;

namespace sisapi.application.Implementations;

public class RoleService(RoleManager<Role> roleManager, CoreDbContext context) : IRoleService
{
    public async Task<ApiResponseDto<RoleDto>> CreateAsync(CreateRoleDto dto)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return ApiResponseDto<RoleDto>.ErrorResponse("Nombre requerido");
        }

        var exists = await context.Roles
            .AnyAsync(r => r.Name == name && r.CompanyId == dto.CompanyId);

        if (exists)
        {
            return ApiResponseDto<RoleDto>.ErrorResponse("Rol con este nombre ya existe en esta empresa");
        }



        if (!await context.Companies.AnyAsync(c => c.Id == dto.CompanyId && c.Active))
        {
            return ApiResponseDto<RoleDto>.ErrorResponse("Empresa no encontrada");
        }

        var role = new Role
        {
            Name = name,
            Description = dto.Description,
            CompanyId = dto.CompanyId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return ApiResponseDto<RoleDto>.ErrorResponse(
                "Failed to create role",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        // Obtener nombre de la compañía asociada (si existe) de forma directa
        string? companyName = null;
        if (role.CompanyId.HasValue)
        {
            var company = await context.Companies.FindAsync(role.CompanyId.Value);
            companyName = company?.Name;
        }

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            Active = role.Active,
            CompanyId = role.CompanyId,
            CompanyName = companyName
        };

        return ApiResponseDto<RoleDto>.SuccessResponse(roleDto, "Rol creado exitosamente");
    }

    public async Task<ApiResponseDto<RoleDto>> UpdateAsync(int id, UpdateRoleDto dto)
{
    var role = await roleManager.FindByIdAsync(id.ToString());
    if (role == null)
    {
        return ApiResponseDto<RoleDto>.ErrorResponse("Rol no encontrado");
    }

    var companyId = dto.CompanyId ?? role.CompanyId;

    if (!string.IsNullOrWhiteSpace(dto.Name))
    {
        var name = dto.Name.Trim();

        var exists = await context.Roles
            .AnyAsync(r => r.Name == name && r.CompanyId == companyId && r.Id != role.Id);

        if (exists)
        {
            return ApiResponseDto<RoleDto>.ErrorResponse("Rol con este nombre ya existe en esta empresa");
        }

        role.Name = name;
    }

    if (dto.Description is not null)
    {
        role.Description = dto.Description;
    }

    if (dto.Active.HasValue)
    {
        role.Active = dto.Active.Value;
    }

    if (dto.CompanyId.HasValue)
    {
        if (!await context.Companies.AnyAsync(c => c.Id == dto.CompanyId.Value && c.Active))
        {
            return ApiResponseDto<RoleDto>.ErrorResponse("Empresa no encontrada");
        }

        role.CompanyId = dto.CompanyId.Value;
    }

    role.UpdatedAt = DateTime.UtcNow;

    var result = await roleManager.UpdateAsync(role);
    if (!result.Succeeded)
    {
        return ApiResponseDto<RoleDto>.ErrorResponse(
            "Failed to update role",
            result.Errors.Select(e => e.Description).ToList()
        );
    }

    string? companyName = null;
    if (role.CompanyId.HasValue)
    {
        var company = await context.Companies.FindAsync(role.CompanyId.Value);
        companyName = company?.Name;
    }

    var roleDto = new RoleDto
    {
        Id = role.Id,
        Name = role.Name ?? string.Empty,
        Description = role.Description,
        Active = role.Active,
        CompanyId = role.CompanyId,
        CompanyName = companyName
    };

    return ApiResponseDto<RoleDto>.SuccessResponse(roleDto, "Rol actualizado exitosamente");
}



    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        var role = await roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Rol no encontrado");
        }

        role.Active = false;
        role.UpdatedAt = DateTime.UtcNow;

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "Failed to delete role",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        return ApiResponseDto<bool>.SuccessResponse(true, "Rol eliminado exitosamente");
    }

    public async Task<ApiResponseDto<bool>> HardDeleteAsync(int id)
    {
        var role = await roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return ApiResponseDto<bool>.ErrorResponse("Rol no encontrado");
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "Fallo al eliminar el rol permanentemente",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        return ApiResponseDto<bool>.SuccessResponse(true, "Rol eliminado permanentemente exitosamente");
    }


    public async Task<ApiResponseDto<RoleDto?>> GetByIdAsync(int id)
    {
        // Usar context.Roles para obtener el role
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
        {
            return ApiResponseDto<RoleDto?>.ErrorResponse("Rol no encontrado");
        }

        string? companyName = null;
        if (role.CompanyId.HasValue)
        {
            var company = await context.Companies.FindAsync(role.CompanyId.Value);
            companyName = company?.Name;
        }

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            Active = role.Active,
            CompanyId = role.CompanyId,
            CompanyName = companyName
        };

        return ApiResponseDto<RoleDto?>.SuccessResponse(roleDto);
    }

    public async Task<ApiResponseDto<RoleDto?>> GetByNameAsync(string name, int companyId)
    {
        var query = context.Roles.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(name))
        {
            name = name.Trim();
            name = name.ToUpper();
            query = query.Where(r => r.NormalizedName == name);
            query = query.Where(r => r.CompanyId == companyId);
        }
        
        var role = await query.FirstOrDefaultAsync();
        if (role == null)
        {
            return ApiResponseDto<RoleDto?>.ErrorResponse("Rol no encontrado");
        }

        return await GetByIdAsync(role.Id);
    }

    public async Task<ApiResponseDto<PaginatedResponseDto<RoleDto>>> GetAllAsync(RoleFilterDto filter)
    {
        // Usar context.Roles para consultar y después mapear company names en lote
        var query = context.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            query = query.Where(r => r.Name!.Contains(filter.Name));
        }

        if (filter.Active.HasValue)
        {
            query = query.Where(r => r.Active == filter.Active.Value);
        }

        if (filter.CompanyId.HasValue)
        {
            query = query.Where(r => r.CompanyId == filter.CompanyId.Value);
        }

        var totalRecords = await query.CountAsync();

        query = !string.IsNullOrWhiteSpace(filter.SortBy)
            ? filter.SortBy.ToLower() switch
            {
                "name" => filter.SortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                "createdat" => filter.SortDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
                _ => query.OrderBy(r => r.Id)
            }
            : query.OrderBy(r => r.Id);

        // Proyectar CompanyName usando left join (GroupJoin + DefaultIfEmpty) para garantizar que EF lo resuelva correctamente
        var projectedQuery = from r in query
                             join c in context.Companies.IgnoreQueryFilters() on r.CompanyId equals c.Id into gj
                             from c in gj.DefaultIfEmpty()
                             select new
                             {
                                 r.Id,
                                 r.Name,
                                 r.Description,
                                 r.Active,
                                 r.CompanyId,
                                 CompanyName = c != null ? c.Name : null
                             };

        var projectedRoles = await projectedQuery
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Rellenar nombres faltantes por si la proyección no los trajo por alguna razón
        var missingCompanyIds = projectedRoles
            .Where(r => r.CompanyId.HasValue && r.CompanyName == null)
            .Select(r => r.CompanyId!.Value)
            .Distinct()
            .ToList();

        Dictionary<int, string> missingCompanies = new();
        if (missingCompanyIds.Any())
        {
            missingCompanies = await context.Companies
                .IgnoreQueryFilters()
                .Where(c => missingCompanyIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);
        }

        var roleDtos = projectedRoles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Description = r.Description,
            Active = r.Active,
            CompanyId = r.CompanyId,
            CompanyName = r.CompanyName ?? (r.CompanyId.HasValue && missingCompanies.TryGetValue(r.CompanyId.Value, out var n) ? n : null)
        }).ToList();

        var paginatedResponse = new PaginatedResponseDto<RoleDto>
        {
            Data = roleDtos,
            TotalRecords = totalRecords,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponseDto<PaginatedResponseDto<RoleDto>>.SuccessResponse(paginatedResponse);
    }
}
