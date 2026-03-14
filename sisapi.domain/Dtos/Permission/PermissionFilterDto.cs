using sisapi.domain.Dtos.Common;

namespace sisapi.domain.Dtos.Permission;

public class PermissionFilterDto : PaginationRequestDto
{
    public string? Code { get; set; }
    public int? Module { get; set; }
    public int? TypePermission { get; set; }
    public bool? Active { get; set; }
}
