namespace sisapi.domain.Dtos.User;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public bool Active { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public List<string> Roles { get; set; } = new();
}