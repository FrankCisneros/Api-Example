using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProject.Models;

[Table("Logs")]
public class LogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}