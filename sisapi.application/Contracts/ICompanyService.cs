using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Company;

namespace sisapi.application.Contracts;

public interface ICompanyService
{
    Task<ApiResponseDto<CompanyDto>> CreateCompanyAsync(CreateCompanyDto dto);
    Task<ApiResponseDto<CompanyDto>> GetCompanyByIdAsync(int id);
    Task<ApiResponseDto<PaginatedResponseDto<CompanyDto>>> GetAllCompaniesAsync(CompanyFilterDto filter);
    Task<ApiResponseDto<CompanyDto>> UpdateCompanyAsync(int id, UpdateCompanyDto dto);
    Task<ApiResponseDto<bool>> DeleteCompanyAsync(int id);
    Task<ApiResponseDto<bool>> AssignUserToCompanyAsync(int userId, int companyId);
}

