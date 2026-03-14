using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.InterestedUser;
using sisapi.domain.Dtos.User;

namespace sisapi.application.Contracts;

public interface IInterestedUserService
{
    Task<ApiResponseDto<InterestedUserResponseDto>> CreateAsync(CreateInterestedUserDto dto);
    Task<ApiResponseDto<PaginatedResponseDto<InterestedUserResponseDto>>> GetAllAsync(InterestedUserFilterDto filter);
    Task<ApiResponseDto<InterestedUserResponseDto>> GetByIdAsync(int id);
    Task<ApiResponseDto<UserDto>> ConvertToUserAsync(ConvertInterestedUserDto dto, string createdBy);
    Task<ApiResponseDto<InterestedUserResponseDto>> UpdateAsync(int id, UpdateInterestedUserDto dto, string updatedBy);
}
