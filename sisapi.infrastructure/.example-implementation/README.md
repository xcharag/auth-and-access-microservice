# Dynamic Permission Implementation Guide for Microservices

## Overview

This guide explains how to implement permission-based authorization in your microservices using the Auth & Access microservice's `verify-permission` endpoint. This approach avoids JWT token bloat by checking permissions on-demand rather than embedding them all in the token.

## Architecture

```
┌─────────────────┐         ┌──────────────────┐         ┌─────────────────┐
│  Microservice   │────────>│  Auth Service    │────────>│   Database      │
│  (SISCON, etc.) │ Verify  │  /verify-perms   │ Check   │  Permissions    │
└─────────────────┘         └──────────────────┘         └─────────────────┘
```

**Benefits:**
- ✅ Keeps JWT tokens small and manageable
- ✅ Centralized permission management
- ✅ Real-time permission updates (no need to refresh token)
- ✅ Support for multiple roles per user
- ✅ Support for different permission types (ControllerAction, MenuOption, etc.)

---

## Step 1: Setup in Your Microservice

### 1.1 Copy Authorization Files

Copy the following files to your microservice's `Infrastructure/Authorization` folder:
- `DynamicPermissionFilter.cs`
- `RequirePermissionAttribute.cs`
- `PermissionAuthorizationHandler.cs`
- `PermissionPolicyProvider.cs`
- `PermissionRequirement.cs`

### 1.2 Copy AuthMicroserviceClient

Copy `AuthMicroserviceClient.cs` to your microservice's `Infrastructure/Services` folder.

### 1.3 Register Services in Program.cs

```csharp
using sisconapi.infrastructure.Authorization;
using sisconapi.infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure HttpClient for Auth Microservice
builder.Services.AddHttpClient<IAuthMicroserviceClient, AuthMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AuthService:Url"] ?? "http://localhost:5282");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    
    // Bypass SSL validation in development (for self-signed certificates)
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    
    return handler;
});

// Add authorization with custom policy provider
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ... rest of your service configuration
```

### 1.4 Configure appsettings.json

```json
{
  "AuthService": {
    "Url": "http://localhost:5282"
  }
}
```

---

## Step 2: Using Dynamic Permissions in Controllers

### Option A: Automatic Permission Checking (Recommended)

Apply `[DynamicPermission]` to the entire controller. The filter will automatically map HTTP methods to permissions:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisconapi.infrastructure.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission] // Automatic permission checking
public class CorrelativoController : ControllerBase
{
    private readonly ICorrelativoService _service;

    public CorrelativoController(ICorrelativoService service)
    {
        _service = service;
    }

    // GET -> Requires SISCON-Correlativo:Read
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CorrelativoFilterDto filter)
    {
        var result = await _service.GetAllCorrelativosAsync(filter);
        return Ok(result);
    }

    // GET {id} -> Requires SISCON-Correlativo:Read
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetCorrelativoByIdAsync(id);
        return Ok(result);
    }

    // POST -> Requires SISCON-Correlativo:Write
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCorrelativoDto dto)
    {
        var result = await _service.CreateCorrelativoAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
    }

    // PUT -> Requires SISCON-Correlativo:Update
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCorrelativoDto dto)
    {
        var result = await _service.UpdateCorrelativoAsync(id, dto);
        return Ok(result);
    }

    // DELETE -> Requires SISCON-Correlativo:Delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteCorrelativoAsync(id);
        return Ok(result);
    }
}
```

**HTTP Method to Action Mapping:**
- `GET` → `Read`
- `POST` → `Write`
- `PUT` / `PATCH` → `Update`
- `DELETE` → `Delete`

### Option B: Specific Permission per Action

Use `[RequirePermission]` for fine-grained control:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    [HttpGet]
    [RequirePermission("SISCON", "Company", "Read")]
    public async Task<IActionResult> GetAll()
    {
        // ...
    }

    [HttpPost]
    [RequirePermission("SISCON", "Company", "Write")]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
    {
        // ...
    }
}
```

### Option C: Different Module or TypePermission

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission(Module.SISAPI, 1)] // Check MenuOption permissions instead
public class MenuController : ControllerBase
{
    // Will check SISAPI-Menu:Read with TypePermission=1 (MenuOption)
    [HttpGet]
    public async Task<IActionResult> GetMenu()
    {
        // ...
    }
}
```

---

## Step 3: Permission Setup in Auth Service

### 3.1 Create Permissions

For each controller in your microservice, create 4 permissions in the Auth & Access database:

```sql
-- Example for Correlativo controller in SISCON module
INSERT INTO Permissions (Code, Module, Description, TypePermission, Active, CreatedAt)
VALUES 
    ('Correlativo', 0, 'Controlador de correlativos', 0, 1, GETDATE()),
    -- TypePermission: 0 = ControllerAction
```

### 3.2 Assign Permissions to Roles

```sql
-- Example: Give "Accountant" role full access to Correlativo
INSERT INTO RolePermissions (RoleId, PermissionId, [Read], [Write], [Update], [Delete], Active, CreatedAt)
VALUES 
    (1, 1, 1, 1, 1, 1, 1, GETDATE());
```

### 3.3 Assign Roles to Users

Users can have multiple roles, and permissions are aggregated from all their roles:

```sql
-- User can have multiple roles
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES 
    (1, 1), -- Accountant role
    (1, 2); -- Manager role
```

---

## Step 4: Testing

### 4.1 Test with Swagger/Postman

1. **Login** to get a JWT token:
   ```http
   POST http://localhost:5282/api/Auth/login
   Content-Type: application/json

   {
     "usernameOrEmail": "admin@example.com",
     "password": "YourPassword123!"
   }
   ```

2. **Call your microservice endpoint** with the token:
   ```http
   GET http://localhost:5000/api/Correlativo
   Authorization: Bearer YOUR_TOKEN_HERE
   ```

3. The microservice will:
   - Extract the token
   - Call `http://localhost:5282/api/Auth/verify-permission?module=SISCON&controller=Correlativo&action=Read&typePermission=0`
   - Return 200 OK if permission granted, or 403 Forbidden if denied

### 4.2 Debug Permission Issues

Enable debug logging in your microservice:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "sisconapi.infrastructure.Authorization": "Debug"
    }
  }
}
```

This will log:
```
[Debug] Checking permission: SISCON-Correlativo:Read (TypePermission: 0)
[Debug] Verifying permission: /api/Auth/verify-permission?module=SISCON&controller=Correlativo&action=Read&typePermission=0
[Info] Permission granted for: SISCON-Correlativo:Read
```

---

## TypePermission Reference

| Value | Name            | Description                          | Example Use Case                    |
|-------|-----------------|--------------------------------------|-------------------------------------|
| 0     | ControllerAction| API endpoint permissions             | GET /api/Users → Read permission    |
| 1     | MenuOption      | UI menu visibility                   | Show "Reports" menu item            |
| 2     | UserAction      | Custom user actions                  | "Export to Excel" button            |
| 3     | ProjectView     | Project-specific visibility          | Access to "Project Alpha"           |

---

## Best Practices

### ✅ DO:
- Use `[DynamicPermission]` at the controller level for automatic permission checking
- Use descriptive controller names that match your permission codes
- Keep permission codes consistent: `Module-Controller:Action`
- Test permission changes immediately (no token refresh needed)
- Use different TypePermission values for different types of access control

### ❌ DON'T:
- Don't mix `[DynamicPermission]` and `[RequirePermission]` on the same controller
- Don't hardcode permission strings in multiple places
- Don't forget to add `[Authorize]` before `[DynamicPermission]`
- Don't cache permission check results for too long (they're checked in real-time)

---

## Troubleshooting

### Issue: 401 Unauthorized
**Cause:** No JWT token or invalid token  
**Solution:** Check that the `Authorization: Bearer TOKEN` header is present and valid

### Issue: 403 Forbidden
**Cause:** User doesn't have the required permission  
**Solution:** 
1. Check the logs to see which permission was required
2. Verify the user has a role with that permission in the Auth database
3. Test the permission directly: `GET /api/Auth/verify-permission?module=X&controller=Y&action=Z`

### Issue: 500 Internal Server Error
**Cause:** Auth microservice is unreachable  
**Solution:** 
1. Check `appsettings.json` has correct `AuthService:Url`
2. Verify the Auth microservice is running
3. Check network connectivity between microservices

---

## Migration from JWT Claim-Based Permissions

If you're currently checking permissions from JWT claims, here's how to migrate:

### Old Approach (JWT Claims):
```csharp
var permissions = User.Claims
    .Where(c => c.Type == "Permission")
    .Select(c => c.Value)
    .ToList();

if (!permissions.Contains("SISCON-Correlativo:Read"))
{
    return Forbid();
}
```

### New Approach (API Call):
```csharp
[DynamicPermission] // That's it! No manual checks needed
```

**Benefits of migration:**
- Smaller JWT tokens
- Real-time permission updates
- No need to refresh tokens when permissions change
- Centralized audit logging

