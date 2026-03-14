using sisapi.domain.Dtos.Report;

namespace sisapi.application.Services.Reports.Strategies;

public interface IReportStrategy
{

    Task<IEnumerable<object>> GetDataAsync(ExcelReportRequest request);

    List<ExcelColumnConfig> GetColumnConfigurations();

    string GetReportName();
}
