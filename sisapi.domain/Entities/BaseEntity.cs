namespace sisapi.domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Active { get; set; } = true;
}
