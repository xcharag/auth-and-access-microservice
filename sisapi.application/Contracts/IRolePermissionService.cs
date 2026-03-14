using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.RolePermission;

namespace sisapi.application.Contracts;

public interface IRolePermissionService
{
    Task<ApiResponseDto<RolePermissionDto>> AssignPermissionToRoleAsync(AssignPermissionToRoleDto dto, int companyId);
    Task<ApiResponseDto<bool>> RemovePermissionFromRoleAsync(int roleId, int permissionId, int companyId);
    Task<ApiResponseDto<List<RolePermissionDto>>> GetRolePermissionsAsync(int roleId, int companyId);
    Task<ApiResponseDto<List<RolePermissionDto>>> GetRolePermissionsFilteredAsync(int roleId, int companyId, int? module, int? typePermission, bool onlyAccounting = false);
    Task<ApiResponseDto<RolePermissionDto>> UpdateRolePermissionAsync(int id, AssignPermissionToRoleDto dto, int companyId);
}