using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Permission;

namespace sisapi.application.Contracts;

public interface IPermissionService
{
    Task<ApiResponseDto<PermissionDto>> CreateAsync(CreatePermissionDto dto);
    Task<ApiResponseDto<PermissionDto>> UpdateAsync(int id, UpdatePermissionDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(int id);
    Task<ApiResponseDto<PermissionDto?>> GetByIdAsync(int id);
    Task<ApiResponseDto<PaginatedResponseDto<PermissionDto>>> GetAllAsync(PermissionFilterDto filter);
    Task<ApiResponseDto<List<PermissionDto>>> GetByModuleAsync(int module);
    Task<ApiResponseDto<List<UserPermissionDto>>> GetUserPermissionsAsync(int userId, int? module, int? typePermission);
}
