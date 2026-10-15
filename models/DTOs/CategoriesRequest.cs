using System.ComponentModel.DataAnnotations;

namespace ApiProject.Models;

public class CategoriesRequest
{
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }
}