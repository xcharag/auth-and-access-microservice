using sisapi.application.Contracts;
using sisapi.domain.Dtos.Report;
using sisapi.domain.Dtos.Role;

namespace sisapi.application.Services.Reports.Strategies;


public class RoleReportStrategy : IReportStrategy
{
    private readonly IRoleService _roleService;

    public RoleReportStrategy(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IEnumerable<object>> GetDataAsync(ExcelReportRequest request)
    {
        var filter = new RoleFilterDto
        {
            SearchTerm = request.SearchTerm,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Active = request.IsActive,
            Name = request.Name
        };

        var result = await _roleService.GetAllAsync(filter);

        if (!result.Success || result.Data == null)
        {
            return Enumerable.Empty<object>();
        }

        var excelData = result.Data.Data.Select(r => new
        {
            Id = r.Id,
            Nombre = r.Name,
            Descripcion = r.Description ?? "",
            Activo = r.Active ? "Sí" : "No"
        });

        return excelData.Cast<object>();
    }

    public List<ExcelColumnConfig> GetColumnConfigurations()
    {
        return new List<ExcelColumnConfig>
        {
            new() { PropertyName = "Id", HeaderText = "ID", Width = 8, Order = 1 },
            new() { PropertyName = "Nombre", HeaderText = "Nombre del Rol", Width = 30, Order = 2 },
            new() { PropertyName = "Descripcion", HeaderText = "Descripción", Width = 50, Order = 3 },
            new() { PropertyName = "Activo", HeaderText = "Activo", Width = 10, Order = 4 }
        };
    }

    public string GetReportName()
    {
        return "Reporte de Roles";
    }
}
