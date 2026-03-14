using System.ComponentModel.DataAnnotations;

namespace sisapi.domain.Dtos.Company;

public class CreateCompanyDto
{
    [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "El NIT no puede superar los 50 caracteres")]
    public string? Nit { get; set; }

    [StringLength(500, ErrorMessage = "La dirección no puede superar los 500 caracteres")]
    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "La ciudad no puede superar los 100 caracteres")]
    public string? City { get; set; }

    [StringLength(100, ErrorMessage = "El estado o provincia no puede superar los 100 caracteres")]
    public string? State { get; set; }

    [StringLength(100, ErrorMessage = "El país no puede superar los 100 caracteres")]
    public string? Country { get; set; }

    [StringLength(20, ErrorMessage = "El código postal no puede superar los 20 caracteres")]
    public string? PostalCode { get; set; }

    [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "El formato del correo es inválido")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "El sitio web no puede superar los 200 caracteres")]
    public string? Website { get; set; }

    [StringLength(500, ErrorMessage = "La URL del logo no puede superar los 500 caracteres")]
    public string? LogoUrl { get; set; }

    [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres")]
    public string? Description { get; set; }
}
