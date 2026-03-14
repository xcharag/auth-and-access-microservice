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
public class PermissionController(IPermissionService permissionService) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePermissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await permissionService.CreateAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermissionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await permissionService.UpdateAsync(id, dto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await permissionService.DeleteAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await permissionService.GetByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("by-module/{module}")]
    public async Task<IActionResult> GetByModule(int module)
    {
        var result = await permissionService.GetByModuleAsync(module);
        
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PermissionFilterDto filter)
    {
        var result = await permissionService.GetAllAsync(filter);
        return Ok(result);
    }

    [HttpGet("user/{userId}/permissions")]
    public async Task<IActionResult> GetUserPermissions(int userId, [FromQuery] int? module, [FromQuery] int? typePermission)
    {
        var result = await permissionService.GetUserPermissionsAsync(userId, module, typePermission);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
