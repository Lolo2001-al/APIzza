using Microsoft.EntityFrameworkCore;
using APIzza.Api.ClasesPizzas;

namespace APIzza.Api.Biblioteca;

/// <summary>
/// Contexto de Entity Framework Core: representa la conexion a la base de datos
/// y expone las tablas (pizzas, clientes, pedidos, items de pedido).
/// </summary>
public class ApizzaDbContext : DbContext
{
    public ApizzaDbContext(DbContextOptions<ApizzaDbContext> opciones) : base(opciones)
    {
    }

    public DbSet<Pizza> Pizzas => Set<Pizza>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItemsPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId);

        modelBuilder.Entity<ItemPedido>()
            .HasOne(i => i.Pedido)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.PedidoId);

        modelBuilder.Entity<ItemPedido>()
            .HasOne(i => i.Pizza)
            .WithMany()
            .HasForeignKey(i => i.PizzaId);

        base.OnModelCreating(modelBuilder);
    }
}
