using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using sisapi.application.Constants;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Company;
using sisapi.domain.Entities;
using sisapi.infrastructure.Context.Core;

namespace sisapi.application.Implementations;

public class CompanyService(CoreDbContext context, UserManager<User> userManager) : ICompanyService
{
    public async Task<ApiResponseDto<CompanyDto>> CreateCompanyAsync(CreateCompanyDto dto)
    {
        var validationErrors = await ValidateCompanyFieldsAsync(dto);
        if (validationErrors.Count > 0)
        {
            return ApiResponseDto<CompanyDto>.ErrorResponse(ApplicationErrorMessages.EmpresaCreacionError, validationErrors);
        }

        var company = new Company
        {
            Name = dto.Name,
            Nit = dto.Nit,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            Phone = dto.Phone,
            Email = dto.Email,
            Website = dto.Website,
            LogoUrl = dto.LogoUrl,
            Description = dto.Description,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();

        var companyDto = MapToDto(company);
        return ApiResponseDto<CompanyDto>.SuccessResponse(companyDto, "Empresa creada correctamente");
    }

    public async Task<ApiResponseDto<CompanyDto>> GetCompanyByIdAsync(int id)
    {
        var company = await context.Companies
            .FirstOrDefaultAsync(c => c.Id == id && c.Active);

        if (company == null)
        {
            return ApiResponseDto<CompanyDto>.ErrorResponse(ApplicationErrorMessages.EmpresaNoEncontrada);
        }

        var companyDto = MapToDto(company);
        return ApiResponseDto<CompanyDto>.SuccessResponse(companyDto, "Empresa obtenida correctamente");
    }

    public async Task<ApiResponseDto<PaginatedResponseDto<CompanyDto>>> GetAllCompaniesAsync(CompanyFilterDto filter)
    {
        var query = context.Companies.AsQueryable();

        if (filter.Active.HasValue)
        {
            query = query.Where(c => c.Active == filter.Active.Value);
        }
        else
        {
            query = query.Where(c => c.Active); 
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= filter.CreatedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(c => c.Name.Contains(filter.SearchTerm) || 
                                     (c.Nit != null && c.Nit.Contains(filter.SearchTerm)) ||
                                     (c.Email != null && c.Email.Contains(filter.SearchTerm)));
        }

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "createdat" => filter.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                "nit" => filter.SortDescending ? query.OrderByDescending(c => c.Nit) : query.OrderBy(c => c.Nit),
                _ => query.OrderBy(c => c.Name)
            };
        }
        else
        {
            query = query.OrderBy(c => c.Name);
        }
        
        var companies = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var companyDtos = companies.Select(MapToDto).ToList();

        var paginatedResponse = new PaginatedResponseDto<CompanyDto>
        {
            Data = companyDtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        };

        return ApiResponseDto<PaginatedResponseDto<CompanyDto>>.SuccessResponse(paginatedResponse, "Listado de empresas obtenido correctamente");
    }

    public async Task<ApiResponseDto<CompanyDto>> UpdateCompanyAsync(int id, UpdateCompanyDto dto)
    {
        var company = await context.Companies.FindAsync(id);
        
        if (company == null || !company.Active)
        {
            return ApiResponseDto<CompanyDto>.ErrorResponse(ApplicationErrorMessages.EmpresaNoEncontrada);
        }

        var validationErrors = await ValidateCompanyFieldsAsync(dto, id);
        if (validationErrors.Count > 0)
        {
            return ApiResponseDto<CompanyDto>.ErrorResponse(ApplicationErrorMessages.EmpresaActualizacionError, validationErrors);
        }

        company.Name = dto.Name;
        company.Nit = dto.Nit;
        company.Address = dto.Address;
        company.City = dto.City;
        company.State = dto.State;
        company.Country = dto.Country;
        company.PostalCode = dto.PostalCode;
        company.Phone = dto.Phone;
        company.Email = dto.Email;
        company.Website = dto.Website;
        company.LogoUrl = dto.LogoUrl;
        company.Description = dto.Description;
        company.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var companyDto = MapToDto(company);
        return ApiResponseDto<CompanyDto>.SuccessResponse(companyDto, "Empresa actualizada correctamente");
    }

    public async Task<ApiResponseDto<bool>> DeleteCompanyAsync(int id)
    {
        var company = await context.Companies.FindAsync(id);
        
        if (company == null || !company.Active)
        {
            return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.EmpresaNoEncontrada);
        }

        company.Active = false;
        company.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return ApiResponseDto<bool>.SuccessResponse(true, "Empresa eliminada correctamente");
    }

    public async Task<ApiResponseDto<bool>> AssignUserToCompanyAsync(int userId, int companyId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.UsuarioNoEncontrado);
        }

        var company = await context.Companies.FindAsync(companyId);
        if (company == null || !company.Active)
        {
            return ApiResponseDto<bool>.ErrorResponse(ApplicationErrorMessages.EmpresaNoEncontrada);
        }

        user.CompanyId = companyId;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponseDto<bool>.ErrorResponse(
                "Failed to assign user to company",
                result.Errors.Select(e => e.Description).ToList()
            );
        }

        return ApiResponseDto<bool>.SuccessResponse(true, "Usuario asignado correctamente a la empresa");
    }

    private static CompanyDto MapToDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Nit = company.Nit,
            Address = company.Address,
            City = company.City,
            State = company.State,
            Country = company.Country,
            PostalCode = company.PostalCode,
            Phone = company.Phone,
            Email = company.Email,
            Website = company.Website,
            LogoUrl = company.LogoUrl,
            Description = company.Description,
            Active = company.Active,
            CreatedAt = company.CreatedAt
        };
    }

    private async Task<List<string>> ValidateCompanyFieldsAsync(CreateCompanyDto dto, int? companyId = null)
    {
        return await ValidateCompanyFieldsAsync(dto.Name, dto.Nit, dto.Email, companyId);
    }

    private async Task<List<string>> ValidateCompanyFieldsAsync(UpdateCompanyDto dto, int? companyId = null)
    {
        return await ValidateCompanyFieldsAsync(dto.Name, dto.Nit, dto.Email, companyId);
    }

    private async Task<List<string>> ValidateCompanyFieldsAsync(string? name, string? nit, string? email, int? companyId = null)
    {
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var existingCompany = await context.Companies
                .FirstOrDefaultAsync(c => c.Name == name && c.Active && (!companyId.HasValue || c.Id != companyId.Value));
            if (existingCompany != null)
            {
                errors.Add(ApplicationErrorMessages.EmpresaNombreDuplicado);
            }
        }

        if (!string.IsNullOrWhiteSpace(nit))
        {
            var existingNit = await context.Companies
                .FirstOrDefaultAsync(c => c.Nit == nit && c.Active && (!companyId.HasValue || c.Id != companyId.Value));
            if (existingNit != null)
            {
                errors.Add(ApplicationErrorMessages.EmpresaNitDuplicado);
            }
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                errors.Add("El formato del correo es inválido");
            }
            else
            {
                var existingEmail = await context.Companies
                    .FirstOrDefaultAsync(c => c.Email == email && c.Active && (!companyId.HasValue || c.Id != companyId.Value));
                if (existingEmail != null)
                {
                    errors.Add(ApplicationErrorMessages.EmpresaEmailDuplicado);
                }
            }
        }

        return errors;
    }
}
