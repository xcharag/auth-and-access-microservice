namespace sisapi.domain.Dtos.RolePermission;

public class RolePermissionDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public bool Read { get; set; }
    public bool Write { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool Active { get; set; }
    public int? CompanyId { get; set; }
}
