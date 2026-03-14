using sisapi.domain.Dtos.Common;

namespace sisapi.domain.Dtos.InterestedUser;

public class InterestedUserFilterDto : PaginationRequestDto
{
    public bool? IsAccepted { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
