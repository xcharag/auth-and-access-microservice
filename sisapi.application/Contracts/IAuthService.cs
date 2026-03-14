using sisapi.domain.Dtos.Auth;
using sisapi.domain.Dtos.Common;

namespace sisapi.application.Contracts;

public interface IAuthService
{
    Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponseDto<bool>> LogoutAsync(string refreshToken);
    Task<ApiResponseDto<bool>> LogoutAllDevicesAsync(int userId);
    Task<ApiResponseDto<bool>> AssignRoleToUserAsync(int userId, string roleName);
    Task<ApiResponseDto<bool>> RemoveRoleFromUserAsync(int userId, string roleName);
    Task<ApiResponseDto<bool>> SoftDeleteUserAsync(int userId);
    Task<ApiResponseDto<bool>> RestoreUserAsync(int userId);
    Task<ApiResponseDto<bool>> VerifyPermissionAsync(int userId, string module, string controller, string action, int typePermission = 0);
}
