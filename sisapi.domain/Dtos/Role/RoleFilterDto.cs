using sisapi.domain.Dtos.Common;

namespace sisapi.domain.Dtos.Role;

public class RoleFilterDto : PaginationRequestDto
{
    public string? Name { get; set; }
    public bool? Active { get; set; }
    public int? CompanyId { get; set; }
}
