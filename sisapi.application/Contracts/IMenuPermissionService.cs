using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Permission;

namespace sisapi.application.Contracts;

public interface IMenuPermissionService
{
    Task<ApiResponseDto<PermissionDto>> CreateAsync(CreateMenuPermissionDto dto);
    Task<ApiResponseDto<IEnumerable<PermissionDto>>> GetAllAsync(int? module = null);
}

