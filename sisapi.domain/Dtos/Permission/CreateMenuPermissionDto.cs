using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.Permission;

public class CreateMenuPermissionDto
{
    [Required(ErrorMessage = "El código es obligatorio")]
    [StringLength(50, ErrorMessage = "El código no puede superar los 50 caracteres")]
    [DefaultValue("MENU_CODE")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El módulo es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "El módulo es inválido")]
    [DefaultValue(0)]
    public int Module { get; set; } = 0;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres")]
    public string? Description { get; set; }
}

