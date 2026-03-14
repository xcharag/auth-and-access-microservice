// sisapi.domain/Entities/User.cs
using Microsoft.AspNetCore.Identity;

namespace sisapi.domain.Entities;

public class User : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? CompanyId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Active { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Navigation properties
    public virtual Company? Company { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    public virtual Person? Person { get; set; }
}