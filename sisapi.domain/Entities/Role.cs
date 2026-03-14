using Microsoft.AspNetCore.Identity;

namespace sisapi.domain.Entities;

public class Role : IdentityRole<int>
{
    public string? Description { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Active { get; set; } = true;
    
    // Relationship
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
