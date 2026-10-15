using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models;

[Table("InventoryLogs")]
public class InventoryLog
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
    public DateTime LogDate { get; set; } = DateTime.Now;
}