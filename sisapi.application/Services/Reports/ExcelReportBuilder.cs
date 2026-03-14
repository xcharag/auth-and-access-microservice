using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using sisapi.domain.Dtos.Report;
using sisapi.application.Services.Reports.Strategies;

namespace sisapi.application.Services.Reports;

public class ExcelReportBuilder : IExcelReportBuilder
{
    private readonly IReportStrategyFactory _strategyFactory;
    private readonly ILogger<ExcelReportBuilder> _logger;

    private const string HeaderBackgroundColor = "#4472C4";
    private const string HeaderFontColor = "#FFFFFF";
    private const string AlternateRowColor = "#F2F2F2";

    public ExcelReportBuilder(
        IReportStrategyFactory strategyFactory,
        ILogger<ExcelReportBuilder> logger)
    {
        _strategyFactory = strategyFactory;
        _logger = logger;
    }

    public async Task<ExcelReportResponse> GenerateAsync(ExcelReportRequest request)
    {
        try
        {
            if (request.PageSize > 50000)
            {
                throw new ArgumentException("El tamaño de página no puede exceder 50,000 registros");
            }

            var strategy = _strategyFactory.GetStrategy(request.ReportType);

            var data = await strategy.GetDataAsync(request);
            var dataList = data.ToList();

            if (!dataList.Any())
            {
                _logger.LogWarning("No data found for report type: {ReportType}", request.ReportType);
            }

            var columnConfigs = request.ColumnConfigurations?.Any() == true
                ? request.ColumnConfigurations
                : strategy.GetColumnConfigurations();

            columnConfigs = columnConfigs.OrderBy(c => c.Order).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(strategy.GetReportName());

            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = strategy.GetReportName();
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
            worksheet.Range(currentRow, 1, currentRow, columnConfigs.Count).Merge();
            currentRow += 2; 

            var headerRow = currentRow;
            for (int i = 0; i < columnConfigs.Count; i++)
            {
                var cell = worksheet.Cell(headerRow, i + 1);
                cell.Value = columnConfigs[i].HeaderText;
                
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.FromHtml(HeaderFontColor);
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(HeaderBackgroundColor);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
            currentRow++;

            int dataRowCount = 0;
            foreach (var item in dataList)
            {
                for (int i = 0; i < columnConfigs.Count; i++)
                {
                    var cell = worksheet.Cell(currentRow, i + 1);
                    var config = columnConfigs[i];
                    
                    var propertyValue = item.GetType().GetProperty(config.PropertyName)?.GetValue(item);
                    
                    if (propertyValue != null)
                    {
                        cell.Value = XLCellValue.FromObject(propertyValue);
                        
                        if (!string.IsNullOrEmpty(config.NumberFormat))
                        {
                            cell.Style.NumberFormat.Format = config.NumberFormat;
                        }
                    }
                    
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    
                    if (dataRowCount % 2 == 1)
                    {
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml(AlternateRowColor);
                    }
                }
                
                currentRow++;
                dataRowCount++;
            }

            if (request.IncludeSummary && dataRowCount > 0)
            {
                currentRow++; 
                var summaryCell = worksheet.Cell(currentRow, 1);
                summaryCell.Value = $"Total de registros: {dataRowCount}";
                summaryCell.Style.Font.Bold = true;
                summaryCell.Style.Font.Italic = true;
                worksheet.Range(currentRow, 1, currentRow, columnConfigs.Count).Merge();
            }

            for (int i = 0; i < columnConfigs.Count; i++)
            {
                var column = worksheet.Column(i + 1);
                if (columnConfigs[i].Width.HasValue && columnConfigs[i].Width.Value > 0)
                {
                    column.Width = columnConfigs[i].Width.Value;
                }
                else
                {
                    column.AdjustToContents();
                }
            }

            worksheet.Range(headerRow, 1, headerRow, columnConfigs.Count).SetAutoFilter();

            worksheet.SheetView.FreezeRows(headerRow);

            var fileName = !string.IsNullOrWhiteSpace(request.FileName)
                ? $"{request.FileName}.xlsx"
                : $"{request.ReportType}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var response = new ExcelReportResponse
            {
                FileName = fileName,
                RowCount = dataRowCount
            };

            if (request.ReturnAsBase64)
            {
                response.Base64Content = Convert.ToBase64String(fileBytes);
            }
            else
            {
                response.FileBytes = fileBytes;
            }

            _logger.LogInformation(
                "Excel report generated successfully. Type: {ReportType}, Rows: {RowCount}",
                request.ReportType, dataRowCount);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel report for type: {ReportType}", request.ReportType);
            throw;
        }
    }
}
