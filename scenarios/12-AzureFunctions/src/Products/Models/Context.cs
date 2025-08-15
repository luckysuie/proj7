using DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Products.Models;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Product => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the float[] property as a vector:
        modelBuilder.Entity<Product>().Property(b => b.Embedding).HasColumnType("vector(1536)");
    }
}
