using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisapi.application.Constants;
using sisapi.application.Contracts;
using sisapi.domain.Dtos.Company;
using sisapi.infrastructure.Authorization;

namespace sisapi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission]
public class CompanyController(ICompanyService companyService) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = ApplicationErrorMessages.DatosInvalidos, errors = modelErrors });
        }

        var result = await companyService.CreateCompanyAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message, errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await companyService.GetCompanyByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CompanyFilterDto filter)
    {
        var result = await companyService.GetAllCompaniesAsync(filter);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = ApplicationErrorMessages.DatosInvalidos, errors = modelErrors });
        }

        var result = await companyService.UpdateCompanyAsync(id, dto);
        
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message, errors = result.Errors });
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await companyService.DeleteCompanyAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("assign-user")]
    public async Task<IActionResult> AssignUser([FromQuery] int userId, [FromQuery] int companyId)
    {
        var result = await companyService.AssignUserToCompanyAsync(userId, companyId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

