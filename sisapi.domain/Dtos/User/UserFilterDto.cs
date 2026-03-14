using sisapi.domain.Dtos.Common;

namespace sisapi.domain.Dtos.User;

public class UserFilterDto : PaginationRequestDto
{
    public int? CompanyId { get; set; }
    public bool? Active { get; set; }
    public bool? IsDeleted { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}