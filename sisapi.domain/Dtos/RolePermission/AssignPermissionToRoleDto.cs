using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.RolePermission;

public class AssignPermissionToRoleDto
{
    [Required(ErrorMessage = "RoleId is required")]
    public int RoleId { get; set; }

    [Required(ErrorMessage = "PermissionId is required")]
    public int PermissionId { get; set; }

    public bool Read { get; set; } = true;
    public bool Write { get; set; } = false;
    public bool Update { get; set; } = false;
    public bool Delete { get; set; } = false;
    public DateTime? ExpirationDate { get; set; }
  
    public int CompanyId { get; set; }
}
