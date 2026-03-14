using sisapi.application.Contracts;
using sisapi.domain.Dtos.Report;
using sisapi.domain.Dtos.User;

namespace sisapi.application.Services.Reports.Strategies;

public class UserReportStrategy : IReportStrategy
{
    private readonly IUserService _userService;

    public UserReportStrategy(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IEnumerable<object>> GetDataAsync(ExcelReportRequest request)
    {
        var filter = new UserFilterDto
        {
            SearchTerm = request.SearchTerm,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Active = request.IsActive,
            CompanyId = request.CompanyId,
            IsDeleted = request.IsDeleted,
            Role = request.Role,
            CreatedFrom = request.CreatedFrom,
            CreatedTo = request.CreatedTo
        };

        var result = await _userService.GetAllAsync(filter);

        if (!result.Success || result.Data == null)
        {
            return Enumerable.Empty<object>();
        }

        var excelData = result.Data.Data.Select(u => new
        {
            Id = u.Id,
            NombreUsuario = u.UserName,
            Email = u.Email,
            Nombre = u.FirstName ?? "",
            Apellido = u.LastName ?? "",
            Telefono = u.PhoneNumber ?? "",
            Empresa = u.CompanyName ?? "",
            Roles = string.Join(", ", u.Roles),
            Activo = u.Active ? "Sí" : "No",
            Eliminado = u.IsDeleted ? "Sí" : "No",
            FechaCreacion = u.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
            CreadoPor = u.CreatedBy ?? "",
            UltimaActualizacion = u.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
            ActualizadoPor = u.UpdatedBy ?? ""
        });

        return excelData.Cast<object>();
    }

    public List<ExcelColumnConfig> GetColumnConfigurations()
    {
        return new List<ExcelColumnConfig>
        {
            new() { PropertyName = "Id", HeaderText = "ID", Width = 8, Order = 1 },
            new() { PropertyName = "NombreUsuario", HeaderText = "Nombre de Usuario", Width = 20, Order = 2 },
            new() { PropertyName = "Email", HeaderText = "Correo Electrónico", Width = 30, Order = 3 },
            new() { PropertyName = "Nombre", HeaderText = "Nombre", Width = 20, Order = 4 },
            new() { PropertyName = "Apellido", HeaderText = "Apellido", Width = 20, Order = 5 },
            new() { PropertyName = "Telefono", HeaderText = "Teléfono", Width = 15, Order = 6 },
            new() { PropertyName = "Empresa", HeaderText = "Empresa", Width = 25, Order = 7 },
            new() { PropertyName = "Roles", HeaderText = "Roles", Width = 30, Order = 8 },
            new() { PropertyName = "Activo", HeaderText = "Activo", Width = 10, Order = 9 },
            new() { PropertyName = "Eliminado", HeaderText = "Eliminado", Width = 10, Order = 10 },
            new() { PropertyName = "FechaCreacion", HeaderText = "Fecha de Creación", Width = 20, Order = 11 },
            new() { PropertyName = "CreadoPor", HeaderText = "Creado Por", Width = 20, Order = 12 },
            new() { PropertyName = "UltimaActualizacion", HeaderText = "Última Actualización", Width = 20, Order = 13 },
            new() { PropertyName = "ActualizadoPor", HeaderText = "Actualizado Por", Width = 20, Order = 14 }
        };
    }

    public string GetReportName()
    {
        return "Reporte de Usuarios";
    }
}
