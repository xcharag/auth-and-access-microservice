using sisapi.domain.Dtos.Auth;

namespace sisapi.application.Contracts;

public interface IClientCredentialsService
{
    Task<AuthResponseDto?> EmitirTokenAsync(string clientId, string clientSecret);
    Task<AuthResponseDto?> RefrescarTokenAsync(string refreshToken);
    Task<bool> RevocarTokenAsync(string refreshToken);
    Task<int?> ObtenerUsuarioPorTokenAsync(string token);
}
