namespace sisapi.domain.Dtos.InterestedUser;

public class InterestedUserResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
    public int? ConvertedToUserId { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
