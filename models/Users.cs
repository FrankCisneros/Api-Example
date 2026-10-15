using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models;

[Table("Users")]
public class Users
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(120)]
    public string? Name { get; set; }
    [Required]
    
    public string? Email { get; set; }
    [Required]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(8)]
    public int number { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}