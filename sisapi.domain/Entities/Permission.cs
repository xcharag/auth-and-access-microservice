namespace sisapi.domain.Entities;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public Enum.Module Module { get; set; }
    public string? Description { get; set; }
    public Enum.TypePermission TypePermission { get; set; }
    
    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
