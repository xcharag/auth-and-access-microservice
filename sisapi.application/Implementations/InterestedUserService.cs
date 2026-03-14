using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sisapi.application.Constants;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Common;
using sisapi.domain.Dtos.Company;
using sisapi.domain.Dtos.InterestedUser;
using sisapi.domain.Dtos.User;
using sisapi.domain.Entities;
using sisapi.infrastructure.Context.Core;

namespace sisapi.application.Implementations;

public class InterestedUserService(
    CoreDbContext context,
    UserManager<User> userManager,
    IPasswordHasher<InterestedUser> passwordHasher)
    : IInterestedUserService
{
    public async Task<ApiResponseDto<InterestedUserResponseDto>> CreateAsync(CreateInterestedUserDto dto)
    {
        try
        {
            if (dto.Password != dto.ConfirmPassword)
            {
                return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse(ApplicationErrorMessages.ConfirmacionPasswordInvalida);
            }

            var existingInterested = await context.InterestedUsers
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Active);
            
            if (existingInterested != null)
            {
                return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse(ApplicationErrorMessages.InteresadoEmailDuplicado);
            }

            var existingUser = await userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse(
                    "A user with this email already exists");
            }

            var interestedUser = new InterestedUser
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                CompanyId = dto.CompanyId,
                Active = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            // Hash the password
            interestedUser.Password = passwordHasher.HashPassword(interestedUser, dto.Password);

            context.InterestedUsers.Add(interestedUser);
            await context.SaveChangesAsync();

            var response = MapToResponseDto(interestedUser);
            return ApiResponseDto<InterestedUserResponseDto>.SuccessResponse(response, InterestedUserMessages.InterestedUserCreated);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse($"Error creating interested user: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<PaginatedResponseDto<InterestedUserResponseDto>>> GetAllAsync(
        InterestedUserFilterDto filter)
    {
        try
        {
            var query = context.InterestedUsers.AsQueryable();

            if (filter.IsAccepted.HasValue)
            {
                query = query.Where(u => u.IsAccepted == filter.IsAccepted.Value);
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(u => u.Email.Contains(filter.SearchTerm) ||
                                         u.FirstName.Contains(filter.SearchTerm) ||
                                         u.LastName.Contains(filter.SearchTerm) ||
                                         (u.PhoneNumber != null && u.PhoneNumber.Contains(filter.SearchTerm)));
            }

            var totalRecords = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "email" => filter.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                    "firstname" => filter.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                    "lastname" => filter.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                    "createdat" => filter.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                    _ => query.OrderByDescending(u => u.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(u => u.CreatedAt);
            }

            var interestedUsers = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var response = interestedUsers.Select(MapToResponseDto).ToList();

            var pagedResult = new PaginatedResponseDto<InterestedUserResponseDto>
            {
                Data = response,
                TotalRecords = totalRecords,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)filter.PageSize)
            };

            return ApiResponseDto<PaginatedResponseDto<InterestedUserResponseDto>>.SuccessResponse(
                pagedResult, InterestedUserMessages.InterestedUsersRetrieved);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<PaginatedResponseDto<InterestedUserResponseDto>>.ErrorResponse(
                $"Error retrieving interested users: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<InterestedUserResponseDto>> GetByIdAsync(int id)
    {
        try
        {
            var interestedUser = await context.InterestedUsers
                .FirstOrDefaultAsync(u => u.Id == id);

            if (interestedUser == null)
            {
                return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse(ApplicationErrorMessages.InteresadoNoEncontrado);
            }

            var response = MapToResponseDto(interestedUser);
            return ApiResponseDto<InterestedUserResponseDto>.SuccessResponse(response, InterestedUserMessages.InterestedUserRetrieved);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse($"Error retrieving interested user: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<UserDto>> ConvertToUserAsync(ConvertInterestedUserDto dto, string createdBy)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Get the interested user
                var interestedUser = await context.InterestedUsers
                    .FirstOrDefaultAsync(u => u.Id == dto.InterestedUserId);

                if (interestedUser == null)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.InteresadoNoEncontrado);
                }

                if (interestedUser.ConvertedToUserId.HasValue)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.InteresadoYaConvertido);
                }

                // Verify company exists
                var company = await context.Companies.FindAsync(dto.CompanyId);
                if (company == null)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.EmpresaNoEncontrada);
                }

                var username = dto.Username ?? 
                               $"{interestedUser.FirstName.ToLower()}.{interestedUser.LastName.ToLower()}{new Random().Next(100, 999)}";

                // Check if username already exists
                var existingUser = await userManager.FindByNameAsync(username);
                if (existingUser != null)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse(ApplicationErrorMessages.UsernameDuplicado);
                }

                var user = new User
                {
                    UserName = username,
                    Email = interestedUser.Email,
                    FirstName = interestedUser.FirstName,
                    LastName = interestedUser.LastName,
                    PhoneNumber = interestedUser.PhoneNumber,
                    CompanyId = dto.CompanyId,
                    EmailConfirmed = false,
                    Active = true,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(user, dto.Password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return ApiResponseDto<UserDto>.ErrorResponse($"Error creating user: {errors}");
                }


                // Mark interested user as converted
                interestedUser.IsAccepted = true;
                interestedUser.Active = false;
                interestedUser.ConvertedToUserId = user.Id;
                interestedUser.ConvertedAt = DateTime.UtcNow;
                interestedUser.UpdatedAt = DateTime.UtcNow;
                interestedUser.UpdatedBy = createdBy;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    CompanyId = user.CompanyId,
                    CompanyName = company.Name,
                    Active = user.Active,
                    IsDeleted = user.IsDeleted,
                    CreatedAt = user.CreatedAt,
                    CreatedBy = user.CreatedBy,
                    Roles = new List<string>()
                };

                return ApiResponseDto<UserDto>.SuccessResponse(userDto, InterestedUserMessages.InterestedUserConverted);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponseDto<UserDto>.ErrorResponse($"{ApplicationErrorMessages.InteresadoConversionError}: {ex.Message}");
            }
        });
    }

    public async Task<ApiResponseDto<InterestedUserResponseDto>> UpdateAsync(int id, UpdateInterestedUserDto dto, string updatedBy)
    {
        try
        {
            var interestedUser = await context.InterestedUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (interestedUser == null)
            {
                return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse(ApplicationErrorMessages.InteresadoNoEncontrado);
            }

            interestedUser.FirstName = dto.FirstName ?? interestedUser.FirstName;
            interestedUser.LastName = dto.LastName ?? interestedUser.LastName;
            interestedUser.PhoneNumber = dto.PhoneNumber ?? interestedUser.PhoneNumber;

            var requestedAcceptance = dto.IsAccepted;
            var companyId = dto.CompanyId ?? interestedUser.CompanyId;

            if (requestedAcceptance == true && !companyId.HasValue)
            {
                return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse(ApplicationErrorMessages.EmpresaIdRequerido);
            }

            interestedUser.UpdatedAt = DateTime.UtcNow;
            interestedUser.UpdatedBy = updatedBy;

            if (requestedAcceptance == true && !interestedUser.ConvertedToUserId.HasValue)
            {
                var convertResult = await ConvertToUserAsync(new ConvertInterestedUserDto
                {
                    InterestedUserId = interestedUser.Id,
                    CompanyId = companyId!.Value,
                    Username = dto.Username,
                    Password = dto.Password
                }, updatedBy);

                if (!convertResult.Success)
                {
                    return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse(convertResult.Message, convertResult.Errors);
                }

                interestedUser.IsAccepted = true;
                interestedUser.Active = false;
            }
            else if (requestedAcceptance.HasValue && requestedAcceptance.Value == false)
            {
                interestedUser.IsAccepted = false;
                interestedUser.Active = true;
                interestedUser.ConvertedToUserId = null;
                interestedUser.ConvertedAt = null;
            }

            await context.SaveChangesAsync();

            var response = MapToResponseDto(interestedUser);
            return ApiResponseDto<InterestedUserResponseDto>.SuccessResponse(response, InterestedUserMessages.InterestedUserRetrieved);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<InterestedUserResponseDto>.ErrorResponse($"Error updating interested user: {ex.Message}");
        }
    }

    private InterestedUserResponseDto MapToResponseDto(InterestedUser interestedUser)
    {
        return new InterestedUserResponseDto
        {
            Id = interestedUser.Id,
            Email = interestedUser.Email,
            FirstName = interestedUser.FirstName,
            LastName = interestedUser.LastName,
            PhoneNumber = interestedUser.PhoneNumber,
            IsAccepted = interestedUser.IsAccepted,
            ConvertedToUserId = interestedUser.ConvertedToUserId,
            ConvertedAt = interestedUser.ConvertedAt,
            CreatedAt = interestedUser.CreatedAt
        };
    }

    private string GenerateTemporaryPassword()
    {
        return $"Temp{Guid.NewGuid().ToString()[..8]}!";
    }
}
