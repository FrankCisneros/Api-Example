using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models;

[Table("Orders")]
public class Orders
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [ForeignKey("ProductId")]
    public Productos? Product { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.Now;

    public ICollection<OrderDetails>? OrderDetails { get; set; }
}