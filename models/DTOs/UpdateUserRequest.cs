using System.ComponentModel.DataAnnotations;

namespace ApiProject.Models;

public class UpdateUserRequest
{
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(0, 99999999)]
    public int Number { get; set; }
}