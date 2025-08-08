// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using EcommerceChatbot.Models;

namespace EcommerceChatbot.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();

     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("numeric");
    }
}
