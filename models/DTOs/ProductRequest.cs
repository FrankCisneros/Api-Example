using System.ComponentModel.DataAnnotations;

namespace ApiProject.Models;

public class ProductRequest
{
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [Required]
    public decimal Price { get; set; }

    [Required]
    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }
}