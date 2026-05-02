using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.Auth;

public class SetPasswordRequestDto
{
    [Required]
    public string NewPassword { get; set; } = string.Empty;
}

