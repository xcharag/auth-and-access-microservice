using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.Role;

public class UpdateRoleDto
{
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string? Name { get; set; }

    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
    public string? Description { get; set; }

    public bool? Active { get; set; }

    public int? CompanyId { get; set; }
}
