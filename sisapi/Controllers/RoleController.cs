using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Role;
using sisapi.infrastructure.Authorization;

namespace sisapi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission]
public class RoleController(IRoleService roleService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await roleService.CreateAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await roleService.UpdateAsync(id, dto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await roleService.DeleteAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:int}/hardDelete")]
    public async Task<IActionResult> HardDelete(int id)
    {
        var result = await roleService.HardDeleteAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await roleService.GetByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("name/{name}/{companyId}")]
    public async Task<IActionResult> GetRoleByName(string name, int companyId)
    {
        var result = await roleService.GetByNameAsync(name, companyId);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] RoleFilterDto filter)
    {
        var result = await roleService.GetAllAsync(filter);
        
        return Ok(result);
    }
}
