using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.User;

public class CreateUserDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }

    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit and one special character")]
    public string Password { get; set; } = string.Empty;

    public int? CompanyId { get; set; }

    public List<string> Roles { get; set; } = new();
}
