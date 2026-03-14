using Microsoft.AspNetCore.Identity;

namespace sisapi.domain.Entities;

public class UserRole : IdentityUserRole<int>
{
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Active { get; set; } = true;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
