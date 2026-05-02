using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.Auth;

public class InternalProvisionUserRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit and one special character")]
    public string Password { get; set; } = string.Empty;

    public string RoleName { get; set; } = "NibuAppUser";
    public bool EmailConfirmed { get; set; } = false;
}
