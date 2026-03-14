using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.Permission;

public class CreatePermissionDto
{
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Module is required")]
    public int Module { get; set; }

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "TypePermission is required")]
    public int TypePermission { get; set; }
}
