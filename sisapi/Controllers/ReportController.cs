using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisapi.domain.Dtos.Report;
using sisapi.infrastructure.Services.Reports;
using sisapi.application.Services.Reports;
using sisapi.infrastructure.Authorization;

namespace sisapi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission]
public class ReportController(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<ReportController> logger,
    IExcelReportBuilder excelReportBuilder)
    : ControllerBase
{
    /// <summary>
    /// Gets a Jasper report with dynamic parameters
    /// </summary>
    /// <param name="request">Report request with path, format, and dynamic parameters</param>
    /// <returns>Report file (PDF, Excel, HTML, etc.) or base64 string</returns>
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateReport([FromBody] ReportRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ReportPath))
            {
                return BadRequest(new { error = "ReportPath is required" });
            }

            // Get Jasper Server configuration
            var jasperServerUrl = configuration["JasperServer:Url"]
                ?? throw new InvalidOperationException("JasperServer URL is not configured");
            var jasperUsername = configuration["JasperServer:Username"]
                ?? throw new InvalidOperationException("JasperServer Username is not configured");
            var jasperPassword = configuration["JasperServer:Password"]
                ?? throw new InvalidOperationException("JasperServer Password is not configured");

            // Create Jasper client
            var jasperClient = new JasperClient(jasperServerUrl, jasperUsername, jasperPassword, httpClientFactory);

            // Get report using XML execution (same as GetJasperReportXML from ReportHelper)
            byte[] reportBytes = await jasperClient.GetReportAsync(
                request.ReportPath,
                request.Parameters,
                request.Format,
                request.IgnorePagination);

            // Return based on requested output type
            if (request.ReturnBase64)
            {
                var base64String = Convert.ToBase64String(reportBytes);
                return Ok(new { data = base64String, format = request.Format });
            }

            var contentType = GetContentType(request.Format);
            var fileName = $"report_{DateTime.Now:yyyyMMddHHmmss}.{request.Format}";
            return File(reportBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating report: {Message}", ex.Message);
            return StatusCode(500, new { error = "Error generating report", details = ex.Message });
        }
    }

    /// <summary>
    /// Generates an Excel report from Entity Framework data with dynamic configuration
    /// </summary>
    /// <param name="request">Excel report request with report type, filters, and formatting options</param>
    /// <returns>Excel file download or base64 string</returns>
    [HttpPost("generate-excel")]
    [ProducesResponseType(typeof(ExcelReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateExcelReport([FromBody] ExcelReportRequest request)
    {
        try
        {
            // Validate report type
            if (!Enum.IsDefined(typeof(ReportType), request.ReportType))
            {
                return BadRequest(new { error = "Tipo de reporte inválido" });
            }

            // Generate report
            var result = await excelReportBuilder.GenerateAsync(request);

            // Return based on requested output type
            if (request.ReturnAsBase64)
            {
                return Ok(new
                {
                    success = true,
                    data = result.Base64Content,
                    fileName = result.FileName,
                    rowCount = result.RowCount,
                    contentType = result.ContentType
                });
            }

            // Return file for download
            return File(result.FileBytes!, result.ContentType, result.FileName);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid request for Excel report generation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating Excel report: {Message}", ex.Message);
            return StatusCode(500, new { error = "Error generando reporte Excel", details = ex.Message });
        }
    }

    private string GetContentType(string format)
    {
        return format.ToLower() switch
        {
            "pdf" => "application/pdf",
            "xls" => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "csv" => "text/csv",
            "html" => "text/html",
            "xml" => "application/xml",
            "rtf" => "application/rtf",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "odt" => "application/vnd.oasis.opendocument.text",
            "ods" => "application/vnd.oasis.opendocument.spreadsheet",
            "json" => "application/json",
            _ => "application/octet-stream"
        };
    }
}

/// <summary>
/// Request model for generating Jasper reports with dynamic parameters
/// </summary>
public class ReportRequest
{
    /// <summary>
    /// Path to the report in JasperServer (e.g., "/reports/MyReport")
    /// </summary>
    public string ReportPath { get; set; } = string.Empty;

    /// <summary>
    /// Output format (pdf, xls, xlsx, csv, html, xml, etc.)
    /// </summary>
    public string Format { get; set; } = "pdf";

    /// <summary>
    /// Dynamic parameters for the report. Each report can have different parameters.
    /// Can be null or empty if the report doesn't require parameters.
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// Whether to return the report as base64 string instead of file download
    /// </summary>
    public bool ReturnBase64 { get; set; } = false;

    /// <summary>
    /// Ignore pagination (default: false)
    /// </summary>
    public bool IgnorePagination { get; set; } = false;
}