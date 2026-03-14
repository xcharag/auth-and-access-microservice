using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Permission;
using sisapi.infrastructure.Authorization;

namespace sisapi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission]
public class ProjectPermissionController(IProjectPermissionService projectPermissionService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectPermissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await projectPermissionService.CreateAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetAll), new { module = dto.Module }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? module)
    {
        var result = await projectPermissionService.GetAllAsync(module);
        return Ok(result);
    }
}