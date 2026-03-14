using Microsoft.Extensions.DependencyInjection;
using sisapi.domain.Dtos.Report;

namespace sisapi.application.Services.Reports.Strategies;


public interface IReportStrategyFactory
{
    IReportStrategy GetStrategy(ReportType reportType);
}


public class ReportStrategyFactory : IReportStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ReportStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IReportStrategy GetStrategy(ReportType reportType)
    {
        return reportType switch
        {
            ReportType.User => _serviceProvider.GetRequiredService<UserReportStrategy>(),
            ReportType.Role => _serviceProvider.GetRequiredService<RoleReportStrategy>(),
            ReportType.Company => _serviceProvider.GetRequiredService<CompanyReportStrategy>(),
            _ => throw new ArgumentException($"No strategy found for report type: {reportType}")
        };
    }
}
