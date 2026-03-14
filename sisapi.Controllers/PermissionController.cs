public class PermissionController(IPermissionService permissionService) : ControllerBase
{
    // ...existing code...
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

    // ...existing code...
}

