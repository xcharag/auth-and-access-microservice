using sisapi.domain.Entities;

namespace sisapi.application.Contracts;

public interface IJwtService
{
    Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(User user);
    Task<(string RefreshToken, DateTime ExpiresAt)> GenerateRefreshTokenAsync(User user, bool rememberMe = false);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken, string? replacedByToken = null);
    Task RevokeAllUserRefreshTokensAsync(int userId);
    Task<List<string>> GetUserPermissionsAsync(User user, int? typePermission = null);
}
