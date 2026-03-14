using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using sisapi.application.Contracts;
using sisapi.domain.Entities;
using sisapi.infrastructure.Context.Core;

namespace sisapi.application.Implementations;

public class JwtService(IConfiguration configuration, UserManager<User> userManager, CoreDbContext context)
    : IJwtService
{
    public async Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(User user)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.CompanyId.HasValue)
        {
            claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiryMinutes = int.TryParse(jwtSettings["ExpiryInMinutes"], out var parsedExpiry)
            ? parsedExpiry
            : 60;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
        
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public async Task<(string RefreshToken, DateTime ExpiresAt)> GenerateRefreshTokenAsync(User user, bool rememberMe = false)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshTokenString = Convert.ToBase64String(randomBytes);

        var refreshMinutes = int.TryParse(jwtSettings["RefreshTokenExpiryInMinutes"], out var parsedRefreshMinutes)
            ? parsedRefreshMinutes
            : 60;
        var rememberDays = int.TryParse(jwtSettings["RememberMeRefreshTokenExpiryInDays"], out var parsedRememberDays)
            ? parsedRememberDays
            : 90;

        var expiresAt = rememberMe
            ? DateTime.UtcNow.AddDays(rememberDays)
            : DateTime.UtcNow.AddMinutes(refreshMinutes);

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return (refreshTokenString, expiresAt);
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        var token = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null)
            return false;

        if (token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow)
            return false;

        if (token.User.IsDeleted)
            return false;

        return true;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Company)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? replacedByToken = null)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token != null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByToken = replacedByToken;
            await context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(int userId)
    {
        var tokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<string>> GetUserPermissionsAsync(User user, int? typePermission = null)
    {
        var roles = await userManager.GetRolesAsync(user);
        
        var query = context.RolePermissions
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => roles.Contains(rp.Role.Name!) 
                         && rp.Active 
                         && rp.Permission.Active
                         && (!rp.ExpirationDate.HasValue || rp.ExpirationDate > DateTime.UtcNow));

        if (typePermission.HasValue)
        {
            query = query.Where(rp => (int)rp.Permission.TypePermission == typePermission.Value);
        }
        
        var permissions = await query
            .Select(rp => new {
                rp.Permission.Code,
                rp.Permission.Module,
                rp.Read,
                rp.Write,
                rp.Update,
                rp.Delete
            })
            .ToListAsync();

        var permissionClaims = new List<string>();

        foreach (var p in permissions)
        {
            if (p.Read) permissionClaims.Add($"{p.Module}-{p.Code}:Read");
            if (p.Write) permissionClaims.Add($"{p.Module}-{p.Code}:Write");
            if (p.Update) permissionClaims.Add($"{p.Module}-{p.Code}:Update");
            if (p.Delete) permissionClaims.Add($"{p.Module}-{p.Code}:Delete");
        }

        return permissionClaims.Distinct().ToList();
    }
}
