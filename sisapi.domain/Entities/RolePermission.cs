namespace sisapi.domain.Entities;

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public bool Read { get; set; }
    public bool Write { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
    public int? CompanyId { get; set; }
    public DateTime? ExpirationDate { get; set; } = null;
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
    public virtual Company? Company { get; set; }
}
