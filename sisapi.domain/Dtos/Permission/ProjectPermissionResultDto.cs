namespace sisapi.domain.Dtos.Permission;

public class ProjectPermissionResultDto
{
    public PermissionDto ProjectViewPermission { get; set; } = null!;
    public PermissionDto DocumentActionPermission { get; set; } = null!;
}

