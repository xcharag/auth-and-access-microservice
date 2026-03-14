namespace sisapi.domain.Dtos.Company;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Nit { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}
