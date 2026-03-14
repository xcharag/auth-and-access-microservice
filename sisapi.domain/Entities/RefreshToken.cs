// sisapi.domain/Entities/RefreshToken.cs
namespace sisapi.domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = null!;
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    
    public virtual User User { get; set; } = null!;
}