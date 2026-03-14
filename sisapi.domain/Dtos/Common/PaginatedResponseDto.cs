namespace sisapi.domain.Dtos.Common;

public class PaginatedResponseDto<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int TotalRecords { get; set; } = 0;
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
