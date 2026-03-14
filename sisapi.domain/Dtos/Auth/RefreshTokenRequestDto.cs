using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.Auth;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}

