# Quick Setup Checklist for Microservices

## 📋 Implementation Checklist

### 1. Copy Files
- [ ] Copy `Authorization/` folder to your microservice's `Infrastructure/` directory
- [ ] Copy `Services/AuthMicroserviceClient.cs` to your microservice's `Infrastructure/Services/`

### 2. Update Program.cs
```csharp
// Add this to your Program.cs
using sisconapi.infrastructure.Authorization;
using sisconapi.infrastructure.Services;

// Configure Auth Microservice Client
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

// Configure Authorization
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

### 3. Update appsettings.json
```json
{
  "AuthService": {
    "Url": "http://localhost:5282"
  }
}
```

### 4. Update Your Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission]  // ← Add this attribute
public class YourController : ControllerBase
{
    // Your actions here
}
```

### 5. Create Permissions in Auth Database
For each controller, create a permission with Code = ControllerName:
```sql
INSERT INTO Permissions (Code, Module, Description, TypePermission, Active, CreatedAt)
VALUES ('YourController', 0, 'Description', 0, 1, GETDATE());
```

### 6. Assign Permissions to Roles
```sql
INSERT INTO RolePermissions (RoleId, PermissionId, [Read], [Write], [Update], [Delete], Active, CreatedAt)
VALUES (RoleId, PermissionId, 1, 1, 1, 1, 1, GETDATE());
```

## ✅ Testing
1. Login to get JWT token
2. Call your microservice endpoint with the token
3. Check logs for permission verification details

## 🚨 Common Issues

| Problem | Solution |
|---------|----------|
| 401 Unauthorized | Add `[Authorize]` before `[DynamicPermission]` |
| 403 Forbidden | User missing required permission in Auth DB |
| 500 Error | Check Auth microservice URL in appsettings.json |
| SSL Connection Error | Add SSL bypass in Program.cs (see Step 2) or use HTTP in appsettings.json |

### SSL Connection Errors
If you see: `The SSL connection could not be established` or `Cannot determine the frame size or a corrupted frame was received`

**Solutions:**
1. ✅ **Use HTTP in Development** (Easiest)
   ```json
   {
     "AuthService": {
       "Url": "http://localhost:5282"
     }
   }
   ```

2. ✅ **Bypass SSL Validation in Development** (Already in Step 2)
   The code in Step 2 includes SSL bypass for development environments.

3. ✅ **Trust the Development Certificate**
   ```powershell
   dotnet dev-certs https --trust
   ```

## 📝 Controller Example

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sisconapi.infrastructure.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DynamicPermission] // Automatically checks permissions
public class ProductController : ControllerBase
{
    // GET /api/Product → Checks SISCON-Product:Read
    [HttpGet]
    public async Task<IActionResult> GetAll() { }

    // POST /api/Product → Checks SISCON-Product:Write
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto) { }

    // PUT /api/Product/{id} → Checks SISCON-Product:Update
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto) { }

    // DELETE /api/Product/{id} → Checks SISCON-Product:Delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) { }
}
```

## 🎯 HTTP Method → Permission Mapping

| HTTP Method | Permission Action |
|-------------|-------------------|
| GET         | Read              |
| POST        | Write             |
| PUT/PATCH   | Update            |
| DELETE      | Delete            |

## 🔐 Permission Format
```
Module-Controller:Action
```

Examples:
- `SISCON-Product:Read`
- `SISCON-Product:Write`
- `SISCON-Product:Update`
- `SISCON-Product:Delete`

## 📞 Auth Service Endpoint
```
GET /api/Auth/verify-permission
  ?module={module}
  &controller={controller}
  &action={action}
  &typePermission={0|1|2|3}
```

## 🎨 TypePermission Values
- `0` = ControllerAction (default)
- `1` = MenuOption
- `2` = UserAction
- `3` = ProjectView

