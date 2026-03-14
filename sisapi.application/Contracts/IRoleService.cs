using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Role;

namespace sisapi.application.Contracts;

public interface IRoleService
{
    Task<ApiResponseDto<RoleDto>> CreateAsync(CreateRoleDto dto);
    Task<ApiResponseDto<RoleDto>> UpdateAsync(int id, UpdateRoleDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(int id);
    Task<ApiResponseDto<bool>> HardDeleteAsync(int id);
    Task<ApiResponseDto<RoleDto?>> GetByIdAsync(int id);
    Task<ApiResponseDto<RoleDto?>> GetByNameAsync(string name, int companyId);
    Task<ApiResponseDto<PaginatedResponseDto<RoleDto>>> GetAllAsync(RoleFilterDto filter);
}
