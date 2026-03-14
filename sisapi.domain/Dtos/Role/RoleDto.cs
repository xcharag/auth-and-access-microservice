namespace sisapi.domain.Dtos.Role;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
}
