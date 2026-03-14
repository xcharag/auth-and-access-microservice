using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.InterestedUser;

public class CreateInterestedUserDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de teléfono es obligatorio")]
    [Phone(ErrorMessage = "El formato del número de teléfono es inválido")]
    public string PhoneNumber { get; set; } = string.Empty;

    public int? CompanyId { get; set; }

    [Compare("Password", ErrorMessage = "La contraseña y su confirmación no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
