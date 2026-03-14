namespace sisapi.domain.Dtos.Report;

/// <summary>
/// Response for Excel report generation
/// </summary>
public class ExcelReportResponse
{
    /// <summary>
    /// Excel file as byte array (if ReturnAsBase64 = false)
    /// </summary>
    public byte[]? FileBytes { get; set; }
    
    /// <summary>
    /// Excel file as base64 string (if ReturnAsBase64 = true)
    /// </summary>
    public string? Base64Content { get; set; }
    
    /// <summary>
    /// Generated filename
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of rows generated (excluding header)
    /// </summary>
    public int RowCount { get; set; }
    
    /// <summary>
    /// Content type for file download
    /// </summary>
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
