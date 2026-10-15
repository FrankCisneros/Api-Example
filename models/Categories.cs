using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models;

[Table("Categories")]
public class Categories
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    public virtual ICollection<Productos>? Productos { get; set; }
}