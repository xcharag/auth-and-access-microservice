using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisapi.application.Constants;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.RolePermission;
using sisapi.infrastructure.Authorization;

namespace sisapi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission]
public class RolePermissionController : ControllerBase
{
    private readonly IRolePermissionService _rolePermissionService;

    public RolePermissionController(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    /// <summary>
    /// Assign a permission to a role with CRUD flags
    /// </summary>
    [HttpPost("assign")]
    public async Task<IActionResult> AssignPermission([FromBody] AssignPermissionToRoleDto dto, [FromQuery] int? companyId)
    {
        if (dto == null)
        {
            return BadRequest(new { message = ApplicationErrorMessages.DatosInvalidos, errors = new { dto = new[] { "The dto field is required." } } });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = ApplicationErrorMessages.DatosInvalidos, errors = ModelState });
        }

        var finalCompanyId = (companyId.HasValue && companyId.Value > 0) ? companyId.Value : dto.CompanyId;

        if (finalCompanyId <= 0)
        {
            return BadRequest(new { message = "CompanyId is required (either as query param or in the dto.CompanyId)" });
        }

        var result = await _rolePermissionService.AssignPermissionToRoleAsync(dto, finalCompanyId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove a permission from a role
    /// </summary>
    [HttpDelete("remove")]
    public async Task<IActionResult> RemovePermission([FromQuery] int roleId, [FromQuery] int permissionId, [FromQuery] int companyId)
    {
        var result = await _rolePermissionService.RemovePermissionFromRoleAsync(roleId, permissionId, companyId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all permissions for a specific role
    /// </summary>
    [HttpGet("role/{roleId}")]
    public async Task<IActionResult> GetRolePermissions(int roleId, [FromQuery] int? companyId)
    {
        var result = await _rolePermissionService.GetRolePermissionsAsync(roleId, companyId ?? 0);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get permissions for a specific role filtered by Module and/or TypePermission
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="module">Module enum value (0=SISAPI)</param>
    /// <param name="typePermission">TypePermission enum value (0=ControllerAction)</param>
    [HttpGet("role/{roleId}/filter")]
    public async Task<IActionResult> GetRolePermissionsFiltered(int roleId, [FromQuery] int? companyId, [FromQuery] int? module, [FromQuery] int? typePermission, [FromQuery] bool onlyAccounting = false)
    {
        var result = await _rolePermissionService.GetRolePermissionsFilteredAsync(roleId, companyId ?? 0, module, typePermission, onlyAccounting);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update role permission CRUD flags
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] AssignPermissionToRoleDto dto, [FromQuery] int? companyId)
    {
        if (dto == null)
        {
            return BadRequest(new { message = ApplicationErrorMessages.DatosInvalidos, errors = new { dto = new[] { "The dto field is required." } } });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = ApplicationErrorMessages.DatosInvalidos, errors = ModelState });
        }

        var finalCompanyId = (companyId.HasValue && companyId.Value > 0) ? companyId.Value : dto.CompanyId;

        if (finalCompanyId <= 0)
        {
            return BadRequest(new { message = "CompanyId is required (either as query param or in the dto.CompanyId)" });
        }

        var result = await _rolePermissionService.UpdateRolePermissionAsync(id, dto, finalCompanyId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
