namespace sisapi.domain.Entities;

public class InterestedUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public bool IsAccepted { get; set; } = false;
    public int? ConvertedToUserId { get; set; }
    public DateTime? ConvertedAt { get; set; }
}
