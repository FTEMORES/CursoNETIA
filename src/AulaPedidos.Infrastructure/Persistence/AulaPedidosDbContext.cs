using AulaPedidos.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace AulaPedidos.Infrastructure.Persistence;

public class AulaPedidosDbContext(DbContextOptions<AulaPedidosDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AulaPedidosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
