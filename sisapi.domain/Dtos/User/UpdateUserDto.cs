using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.User;

public class UpdateUserDto
{
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string? UserName { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
    public string? Email { get; set; }
    
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }
    
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }
    
    public int? CompanyId { get; set; }
    
    public bool? Active { get; set; }
    public List<string>? Roles { get; set; }

    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}