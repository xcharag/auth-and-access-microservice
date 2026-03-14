namespace sisapi.domain.Dtos.Report;

/// <summary>
/// Request for generating Excel reports
/// </summary>
public class ExcelReportRequest
{
    /// <summary>
    /// Type of report to generate
    /// </summary>
    public ReportType ReportType { get; set; }
    
    /// <summary>
    /// Search term for filtering
    /// </summary>
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Page size for pagination (default: 1000, max recommended: 50000)
    /// </summary>
    public int PageSize { get; set; } = 1000;
    
    /// <summary>
    /// Filter by active status (null = all, true = active only, false = inactive only)
    /// </summary>
    public bool? IsActive { get; set; }
    
    // Additional filters for specific entities
    
    /// <summary>
    /// Filter by company ID (User reports)
    /// </summary>
    public int? CompanyId { get; set; }
    
    /// <summary>
    /// Filter by deleted status (User reports)
    /// </summary>
    public bool? IsDeleted { get; set; }
    
    /// <summary>
    /// Filter by role name (User reports)
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// Filter by name (Role reports)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Filter by creation date from (User/Company reports)
    /// </summary>
    public DateTime? CreatedFrom { get; set; }
    
    /// <summary>
    /// Filter by creation date to (User/Company reports)
    /// </summary>
    public DateTime? CreatedTo { get; set; }
    
    /// <summary>
    /// Return as base64 string instead of file download
    /// </summary>
    public bool ReturnAsBase64 { get; set; } = false;
    
    /// <summary>
    /// Custom filename (without extension). If null, auto-generated
    /// </summary>
    public string? FileName { get; set; }
    
    /// <summary>
    /// Include summary/totals row at the end
    /// </summary>
    public bool IncludeSummary { get; set; } = true;
    
    /// <summary>
    /// Custom column configurations (optional - uses defaults if not provided)
    /// </summary>
    public List<ExcelColumnConfig>? ColumnConfigurations { get; set; }
}
