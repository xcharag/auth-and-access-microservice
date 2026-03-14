using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.InterestedUser;

public class ConvertInterestedUserDto
{
    [Required(ErrorMessage = "El identificador del interesado es obligatorio")]
    public int InterestedUserId { get; set; }

    [Required(ErrorMessage = "El identificador de la empresa es obligatorio")]
    public int CompanyId { get; set; }

    public string? Username { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = string.Empty;
}
