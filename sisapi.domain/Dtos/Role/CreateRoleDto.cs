using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.Role;

public class CreateRoleDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "CompanyId is required")]
    public int CompanyId { get; set; }
}
