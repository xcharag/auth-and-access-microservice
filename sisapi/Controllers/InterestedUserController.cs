using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisapi.application.Contracts;
using sisapi.application.Constants;
using sisapi.domain.Dtos.InterestedUser;
using sisapi.infrastructure.Authorization;

namespace sisapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterestedUserController(IInterestedUserService interestedUserService) : ControllerBase
{
    [HttpPost("register-interest")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterInterest([FromBody] CreateInterestedUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = ApplicationErrorMessages.DatosInvalidos, errors = ModelState });
        }

        var result = await interestedUserService.CreateAsync(dto);

        if (!result.Success)
        {
            return BadRequest(new { message = ApplicationErrorMessages.InteresadoCreacionError, errors = result.Errors });
        }

        return Ok(new
        {
            message = InterestedUserMessages.InterestedUserCreated,
            data = result.Data
        });
    }
    
    [HttpGet]
    [Authorize]
    [DynamicPermission]
    public async Task<IActionResult> GetAll([FromQuery] InterestedUserFilterDto filter)
    {
        var result = await interestedUserService.GetAllAsync(filter);

        if (!result.Success)
        {
            return BadRequest(new { message = ApplicationErrorMessages.InteresadosConsultaError, errors = result.Errors });
        }

        return Ok(new
        {
            message = InterestedUserMessages.InterestedUsersRetrieved,
            data = result.Data
        });
    }

    [HttpGet("{id}")]
    [Authorize]
    [DynamicPermission]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await interestedUserService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(new { message = ApplicationErrorMessages.InteresadoConsultaError, errors = result.Errors });
        }

        return Ok(new
        {
            message = InterestedUserMessages.InterestedUserRetrieved,
            data = result.Data
        });
    }
    
    [HttpPost("convert-to-user")]
    [Authorize]
    [DynamicPermission]
    public async Task<IActionResult> ConvertToUser([FromBody] ConvertInterestedUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = ApplicationErrorMessages.DatosInvalidos, errors = ModelState });
        }

        var createdBy = User.Identity?.Name ?? "System";
        var result = await interestedUserService.ConvertToUserAsync(dto, createdBy);

        if (!result.Success)
        {
            var message = string.IsNullOrWhiteSpace(result.Message)
                ? ApplicationErrorMessages.InteresadoConversionError
                : result.Message;
            return BadRequest(new { message, errors = result.Errors });
        }

        return Ok(new
        {
            message = InterestedUserMessages.InterestedUserConverted,
            data = result.Data
        });
    }
    
    [HttpPut("{id}")]
    [Authorize]
    [DynamicPermission]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInterestedUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = ApplicationErrorMessages.DatosInvalidos, errors = ModelState });
        }

        var updatedBy = User.Identity?.Name ?? "System";
        var result = await interestedUserService.UpdateAsync(id, dto, updatedBy);

        if (!result.Success)
        {
            var message = string.IsNullOrWhiteSpace(result.Message)
                ? ApplicationErrorMessages.InteresadoConversionError
                : result.Message;
            return BadRequest(new { message, errors = result.Errors });
        }

        return Ok(new
        {
            message = InterestedUserMessages.InterestedUserUpdated,
            data = result.Data
        });
    }
}
