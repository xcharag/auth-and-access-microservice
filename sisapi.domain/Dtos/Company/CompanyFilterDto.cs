using sisapi.domain.Dtos.Common;

namespace sisapi.domain.Dtos.Company;

public class CompanyFilterDto : PaginationRequestDto
{
    public bool? Active { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}

