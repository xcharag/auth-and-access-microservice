namespace sisapi.domain.Dtos.Permission;

public class UserPermissionDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TypePermission { get; set; } = string.Empty;
    public bool Read { get; set; }
    public bool Write { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

