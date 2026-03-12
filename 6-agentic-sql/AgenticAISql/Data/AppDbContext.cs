using AgenticAISql.Models;
using Microsoft.EntityFrameworkCore;

namespace AgenticAISql.Data;

public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "Ada Lovelace", City = "London" },
            new Customer { Id = 2, Name = "Alan Turing", City = "Manchester" },
            new Customer { Id = 3, Name = "Grace Hopper", City = "New York" }
        );

        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, CustomerId = 1, Total = 1200m, Created = new DateTime(2024, 1, 10) },
            new Order { Id = 2, CustomerId = 1, Total = 300m, Created = new DateTime(2024, 2, 12) },
            new Order { Id = 3, CustomerId = 2, Total = 800m, Created = new DateTime(2024, 1, 5) },
            new Order { Id = 4, CustomerId = 3, Total = 1500m, Created = new DateTime(2024, 3, 1) }
        );
    }
}