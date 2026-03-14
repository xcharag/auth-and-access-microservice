using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.InterestedUser;

public class UpdateInterestedUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsAccepted { get; set; }
    public int? CompanyId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
