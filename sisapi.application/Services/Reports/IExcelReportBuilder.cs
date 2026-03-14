using sisapi.domain.Dtos.Report;

namespace sisapi.application.Services.Reports;

/// <summary>
/// Service for building Excel reports from data
/// </summary>
public interface IExcelReportBuilder
{
    /// <summary>
    /// Generates an Excel report based on the request
    /// </summary>
    Task<ExcelReportResponse> GenerateAsync(ExcelReportRequest request);
}
