using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.User;

namespace sisapi.application.Contracts;

public interface IUserService
{
    Task<ApiResponseDto<UserDto>> CreateAsync(CreateUserDto dto);
    Task<ApiResponseDto<UserDto>> UpdateAsync(int id, UpdateUserDto dto);
    Task<ApiResponseDto<bool>> SoftDeleteAsync(int id);
    Task<ApiResponseDto<bool>> RestoreAsync(int id);
    Task<ApiResponseDto<UserDto?>> GetByIdAsync(int id);
    Task<ApiResponseDto<PaginatedResponseDto<UserDto>>> GetAllAsync(UserFilterDto filter);
    Task<ApiResponseDto<List<UserDto>>> GetUsersByCompanyAsync(int companyId);
    Task<ApiResponseDto<bool>> AssignRoleAsync(int userId, string roleName);
    Task<ApiResponseDto<bool>> RemoveRoleAsync(int userId, string roleName);
}