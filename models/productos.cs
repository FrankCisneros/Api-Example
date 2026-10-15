using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models;

[Table("Productos")]
public class Productos
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(120)]
    public string? Name { get; set; }
    
    [Required]
    public decimal Price { get; set; }

    [Required]
    public string? Description { get; set; }

    [Required]
    public int Stock { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Categories? Category { get; set; }

    public ICollection<Orders>? Orders { get; set; }
    public ICollection<OrderDetails>? OrderDetails { get; set; }
    public ICollection<InventoryLog>? InventoryLogs { get; set; }
}