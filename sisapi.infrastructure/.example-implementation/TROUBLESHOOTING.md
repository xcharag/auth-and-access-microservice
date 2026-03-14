# Troubleshooting Guide

## SSL Connection Issues

### Error: "The SSL connection could not be established"
```
System.Net.Http.HttpRequestException: The SSL connection could not be established, see inner exception.
---> System.Security.Authentication.AuthenticationException: Cannot determine the frame size or a corrupted frame was received.
```

**Cause:** The HttpClient is trying to connect to the Auth service over HTTPS with a self-signed certificate that isn't trusted.

**Solutions (in order of recommendation):**

#### 1. Use HTTP in Development (Easiest) ⭐
Update your microservice's `appsettings.Development.json`:
```json
{
  "AuthService": {
    "Url": "http://localhost:5282"
  }
}
```

Make sure the Auth service (sisapi) is also listening on HTTP. Check `sisapi/Properties/launchSettings.json`.

#### 2. Bypass SSL Validation in Development (Recommended)
In your microservice's `Program.cs`, configure the HttpClient with SSL bypass:

```csharp
builder.Services.AddHttpClient<IAuthMicroserviceClient, AuthMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AuthService:Url"] ?? "http://localhost:5282");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    
    // Only bypass SSL in development - NEVER in production!
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    
    return handler;
});
```

**Benefits:**
- Works with HTTPS
- Only bypasses SSL in development
- Production will still validate certificates properly

#### 3. Trust the Development Certificate
Run this command to trust the ASP.NET Core development certificate:
```powershell
dotnet dev-certs https --trust
```

If that doesn't work, try regenerating the certificate:
```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

## Authorization Issues

### Error: 401 Unauthorized
**Symptom:** Request is rejected before checking permissions.

**Causes & Solutions:**
1. **Missing JWT Token**
   - Ensure you're sending the token in the Authorization header: `Bearer {token}`
   
2. **Invalid Token**
   - Token may be expired (check `exp` claim)
   - Token may be malformed
   - Verify JWT secret matches between Auth service and microservice

3. **Missing [Authorize] Attribute**
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   [Authorize] // ← Make sure this is present
   [DynamicPermission]
   public class YourController : ControllerBase { }
   ```

### Error: 403 Forbidden
**Symptom:** Request passes authentication but fails permission check.

**Causes & Solutions:**
1. **User Missing Permission in Database**
   - Check if the permission exists in the `Permissions` table
   - Check if the user's role has the permission in `RolePermissions` table
   - Verify the permission action (Read/Write/Update/Delete) is set to `1` (true)

2. **Permission Code Mismatch**
   - Permission code must match the controller name exactly
   - Example: For `BancoController`, the permission code should be `Banco`

3. **Wrong Module Name**
   - Verify the module name in the controller matches the permission's module
   - Default is extracted from JWT token's `module` claim

**Check Query:**
```sql
-- Verify user has the permission
SELECT u.UserName, r.Name as RoleName, p.Code, p.Module, 
       rp.[Read], rp.[Write], rp.[Update], rp.[Delete]
FROM Users u
JOIN UserRoles ur ON u.Id = ur.UserId
JOIN Roles r ON ur.RoleId = r.Id
JOIN RolePermissions rp ON r.Id = rp.RoleId
JOIN Permissions p ON rp.PermissionId = p.Id
WHERE u.UserName = 'your-username' 
  AND p.Code = 'Banco'
  AND p.Module = 0  -- SISCON
  AND rp.[Read] = 1
```

---

## Connection Issues

### Error: "Connection refused" or "Cannot connect to Auth service"
**Causes & Solutions:**

1. **Auth Service Not Running**
   - Start the sisapi (Auth & Access) service
   - Verify it's running on the expected port

2. **Wrong URL in Configuration**
   - Check `appsettings.json` or `appsettings.Development.json`
   - Ensure the URL matches where the Auth service is running
   ```json
   {
     "AuthService": {
       "Url": "http://localhost:5282"  // Verify this port
     }
   }
   ```

3. **Port Conflict**
   - Check if another service is using the port
   - Try using a different port in both services

---

## Token Issues

### Token Too Large
If your JWT token is growing too large with many permissions embedded:

**Solution:** Use dynamic permission checking (which this implementation already does!)
- Permissions are checked on-demand via the `/verify-permission` endpoint
- Only essential claims (userId, email, module) are in the token
- Permission claims are NOT added to the token

### Token Expired
**Symptom:** 401 Unauthorized after some time

**Solutions:**
1. Implement token refresh mechanism
2. Increase token expiration time (for development only)
3. Re-login to get a new token

---

## Database Issues

### Missing Permissions
**Symptom:** All requests to a controller return 403 Forbidden

**Solution:** Create permissions for the controller
```sql
-- Create permission (adjust Code and Module as needed)
INSERT INTO Permissions (Code, Module, Description, TypePermission, Active, CreatedAt)
VALUES ('YourController', 0, 'Your Controller Permissions', 0, 1, GETDATE());

-- Assign to role (adjust RoleId)
DECLARE @PermissionId INT = SCOPE_IDENTITY();
INSERT INTO RolePermissions (RoleId, PermissionId, [Read], [Write], [Update], [Delete], Active, CreatedAt)
VALUES (1, @PermissionId, 1, 1, 1, 1, 1, GETDATE());
```

### User Has No Roles
**Symptom:** User authenticated but has no permissions

**Solution:** Assign user to a role
```sql
-- Check user's roles
SELECT u.UserName, r.Name as RoleName
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id
WHERE u.UserName = 'your-username';

-- Assign role if missing
INSERT INTO UserRoles (UserId, RoleId)
VALUES ('user-id-here', 1);  -- Adjust RoleId as needed
```

---

## Debugging Tips

### Enable Detailed Logging
In your microservice's `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "sisconapi.infrastructure.Authorization": "Debug",
      "sisconapi.infrastructure.Services": "Debug"
    }
  }
}
```

### Check What's Being Sent to Auth Service
The logs will show:
```
Verifying permission - Module: SISCON, Controller: Banco, Action: Read
```

### Verify JWT Token Contents
Decode your token at [jwt.io](https://jwt.io) and check:
- `userId` claim is present
- `email` claim is present
- `module` claim is present (if using multi-module setup)
- Token is not expired (`exp` claim)

### Test Auth Service Directly
Use a tool like Postman or curl:
```powershell
# Get token
$token = "your-jwt-token-here"

# Test verify-permission endpoint
curl -X GET "http://localhost:5282/api/Auth/verify-permission?module=SISCON&controller=Banco&action=Read&typePermission=0" `
  -H "Authorization: Bearer $token"
```

Expected response:
```json
{
  "success": true,
  "message": "Permiso verificado correctamente",
  "data": true
}
```

---

## Performance Issues

### Slow Permission Checks
If permission verification is slow:

1. **Add Database Indexes**
   ```sql
   -- Ensure these indexes exist
   CREATE INDEX IX_Permissions_Code_Module ON Permissions(Code, Module);
   CREATE INDEX IX_RolePermissions_RoleId_PermissionId ON RolePermissions(RoleId, PermissionId);
   CREATE INDEX IX_UserRoles_UserId ON UserRoles(UserId);
   ```

2. **Enable Response Caching** (if permissions don't change frequently)
   Consider caching permission check results for a short time (e.g., 1-5 minutes)

3. **Connection Pooling**
   Ensure HttpClient is registered with dependency injection (not created per request)

---

## Still Having Issues?

1. Check all logs (both microservice and Auth service)
2. Verify database connectivity
3. Ensure all migrations are applied
4. Check firewall/network settings
5. Verify the Auth service is accessible from the microservice

### Quick Diagnostic Checklist
- [ ] Auth service is running
- [ ] Auth service URL is correct in appsettings.json
- [ ] SSL configuration is correct (HTTP or SSL bypass)
- [ ] JWT token is valid and not expired
- [ ] User has at least one role
- [ ] Role has the required permission
- [ ] Permission action (Read/Write/Update/Delete) is enabled
- [ ] Permission is active
- [ ] HttpClient is properly configured in Program.cs

