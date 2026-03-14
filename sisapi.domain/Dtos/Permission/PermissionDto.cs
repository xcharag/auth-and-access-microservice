namespace sisapi.domain.Dtos.Permission;

public class PermissionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TypePermission { get; set; } = string.Empty;
    public bool Active { get; set; }
}
