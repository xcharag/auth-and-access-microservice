using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sisapi.domain.Enum;

namespace sisapi.domain.Entities;

public class Person
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string FirstName { get; set; } = string.Empty;
    
    [MaxLength(150)]
    public string? SecondName { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? SurName { get; set; }

    [Required] public TypeDocument TypeDocument { get; set; } = TypeDocument.Ci;
    
    [Required]
    [MaxLength(20)]
    public string DocumentNumber { get; set; } = string.Empty;
    
    [MaxLength(10)]
    public string? DocumentException { get; set; } = string.Empty;
    
    [MaxLength(10)]
    public string? DocumentExpedition { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    public Gender Gender { get; set; } = Gender.Other;
    
    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }
}