using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.User;
using sisapi.infrastructure.Authorization;

namespace sisapi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission]
public class UserController(IUserService userService) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await userService.CreateAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await userService.UpdateAsync(id, dto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await userService.GetByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserFilterDto filter)
    {
        var result = await userService.GetAllAsync(filter);
        
        return Ok(result);
    }

    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetByCompany(int companyId)
    {
        var result = await userService.GetUsersByCompanyAsync(companyId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var result = await userService.SoftDeleteAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var result = await userService.RestoreAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/roles/{roleName}")]
    public async Task<IActionResult> AssignRole(int id, string roleName)
    {
        // Prevent users assigning roles to themselves via this endpoint
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var currentUserId) && currentUserId == id)
        {
            return BadRequest(new { Success = false, Message = "No se permite asignarse roles a sí mismo." });
        }

        var result = await userService.AssignRoleAsync(id, roleName);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}/roles/{roleName}")]
    public async Task<IActionResult> RemoveRole(int id, string roleName)
    {
        var result = await userService.RemoveRoleAsync(id, roleName);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}