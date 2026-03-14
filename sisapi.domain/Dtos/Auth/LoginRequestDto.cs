using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Debe ingresar el usuario o correo")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar la contraseña")]
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; } = false;
}
