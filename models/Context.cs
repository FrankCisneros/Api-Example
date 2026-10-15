using Microsoft.EntityFrameworkCore;

namespace ApiProject.Models;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    { }

    // Tablas de usuario
    public DbSet<Users> Users { get; set; }

    // Tablas de productos
    public DbSet<Productos> Productos { get; set; }
    public DbSet<Categories> Categories { get; set; }
    
    // Tablas de inventario
    public DbSet<Orders> Orders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }
    public DbSet<InventoryLog> InventoryLogs { get; set; }

    // Logs
    public DbSet<LogEntry> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderDetails>()
            .HasOne(detail => detail.Product)
            .WithMany(product => product.OrderDetails)
            .HasForeignKey(detail => detail.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Orders>()
            .HasOne(order => order.Product)
            .WithMany(product => product.Orders)
            .HasForeignKey(order => order.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Productos>()
            .Property(product => product.Price)
            .HasPrecision(18, 2);
    }
}