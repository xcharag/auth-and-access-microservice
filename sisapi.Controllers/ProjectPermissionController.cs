// ...existing code...
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
// ...existing code...

