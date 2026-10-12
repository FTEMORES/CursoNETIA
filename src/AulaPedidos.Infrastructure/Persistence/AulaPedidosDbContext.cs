using Microsoft.EntityFrameworkCore;

namespace AulaPedidos.Infrastructure.Persistence;

public class AulaPedidosDbContext(DbContextOptions<AulaPedidosDbContext> options) : DbContext(options)
{
}