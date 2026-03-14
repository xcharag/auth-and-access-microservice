namespace sisapi.domain.Dtos.Report;

/// <summary>
/// Configuration for Excel column formatting
/// </summary>
public class ExcelColumnConfig
{
    /// <summary>
    /// Property name from the data object
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Display header text (Spanish by default)
    /// </summary>
    public string HeaderText { get; set; } = string.Empty;
    
    /// <summary>
    /// Column width (null = auto-fit)
    /// </summary>
    public double? Width { get; set; }
    
    /// <summary>
    /// ClosedXML number format string (e.g., "dd/MM/yyyy", "#,##0.00")
    /// </summary>
    public string? NumberFormat { get; set; }
    
    /// <summary>
    /// Order of column (left to right)
    /// </summary>
    public int Order { get; set; }
}
