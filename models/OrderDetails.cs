using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiProject.Models;

[Table("OrderDetails")]
public class OrderDetails
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    
    [ForeignKey("OrderId")]
    [JsonIgnore]
    public Orders? Order { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [ForeignKey("ProductId")]
    public Productos? Product { get; set; }
    
    [Required]
    public int Quantity { get; set; }
}