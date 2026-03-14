namespace sisapi.domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Nit { get; set; } // RNC/NIF/Tax Identification Number
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
