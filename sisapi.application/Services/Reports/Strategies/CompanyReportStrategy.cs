using sisapi.application.Contracts;
using sisapi.domain.Dtos.Company;
using sisapi.domain.Dtos.Report;

namespace sisapi.application.Services.Reports.Strategies;

public class CompanyReportStrategy : IReportStrategy
{
    private readonly ICompanyService _companyService;

    public CompanyReportStrategy(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    public async Task<IEnumerable<object>> GetDataAsync(ExcelReportRequest request)
    {
        var filter = new CompanyFilterDto
        {
            SearchTerm = request.SearchTerm,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Active = request.IsActive,
            CreatedFrom = request.CreatedFrom,
            CreatedTo = request.CreatedTo
        };

        var result = await _companyService.GetAllCompaniesAsync(filter);

        if (!result.Success || result.Data == null)
        {
            return Enumerable.Empty<object>();
        }

        var excelData = result.Data.Data.Select(c => new
        {
            c.Id,
            Nombre = c.Name,
            NIT = c.Nit ?? "",
            Direccion = c.Address ?? "",
            Ciudad = c.City ?? "",
            Estado = c.State ?? "",
            Pais = c.Country ?? "",
            CodigoPostal = c.PostalCode ?? "",
            Telefono = c.Phone ?? "",
            Email = c.Email ?? "",
            SitioWeb = c.Website ?? "",
            Descripcion = c.Description ?? "",
            Activo = c.Active ? "Sí" : "No",
            FechaCreacion = c.CreatedAt.ToString("dd/MM/yyyy HH:mm")
        });

        return excelData.Cast<object>();
    }

    public List<ExcelColumnConfig> GetColumnConfigurations()
    {
        return new List<ExcelColumnConfig>
        {
            new() { PropertyName = "Id", HeaderText = "ID", Width = 8, Order = 1 },
            new() { PropertyName = "Nombre", HeaderText = "Nombre de Empresa", Width = 30, Order = 2 },
            new() { PropertyName = "NIT", HeaderText = "NIT", Width = 15, Order = 3 },
            new() { PropertyName = "Direccion", HeaderText = "Dirección", Width = 35, Order = 4 },
            new() { PropertyName = "Ciudad", HeaderText = "Ciudad", Width = 20, Order = 5 },
            new() { PropertyName = "Estado", HeaderText = "Estado/Departamento", Width = 20, Order = 6 },
            new() { PropertyName = "Pais", HeaderText = "País", Width = 15, Order = 7 },
            new() { PropertyName = "CodigoPostal", HeaderText = "Código Postal", Width = 12, Order = 8 },
            new() { PropertyName = "Telefono", HeaderText = "Teléfono", Width = 15, Order = 9 },
            new() { PropertyName = "Email", HeaderText = "Correo Electrónico", Width = 30, Order = 10 },
            new() { PropertyName = "SitioWeb", HeaderText = "Sitio Web", Width = 30, Order = 11 },
            new() { PropertyName = "Descripcion", HeaderText = "Descripción", Width = 40, Order = 12 },
            new() { PropertyName = "Activo", HeaderText = "Activo", Width = 10, Order = 13 },
            new() { PropertyName = "FechaCreacion", HeaderText = "Fecha de Creación", Width = 20, Order = 14 }
        };
    }

    public string GetReportName()
    {
        return "Reporte de Empresas";
    }
}
