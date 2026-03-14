using sisapi.domain.Dtos.Permission;
using sisapi.domain.Dtos.Common;

namespace sisapi.application.Contracts;

public interface IProjectPermissionService
{
    Task<ApiResponseDto<PermissionDto>> CreateAsync(CreateProjectPermissionDto dto);
    Task<ApiResponseDto<IEnumerable<PermissionDto>>> GetAllAsync(int? module = null);
}