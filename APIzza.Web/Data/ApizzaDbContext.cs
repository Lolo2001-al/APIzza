using APIzza.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace APIzza.Web.Data;

/// <summary>Conexion a MySQL y mapeo de las tablas del sistema.</summary>
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
        modelBuilder.Entity<Pizza>(entity =>
        {
            entity.Property(p => p.Precio).HasPrecision(10, 2);
            entity.Property(p => p.Categoria).HasMaxLength(30);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(c => c.Email).IsUnique();
            entity.Property(c => c.Nombre).HasMaxLength(120);
            entity.Property(c => c.Email).HasMaxLength(160);
            entity.Property(c => c.Telefono).HasMaxLength(40);
            entity.Property(c => c.Direccion).HasMaxLength(250);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(p => p.Estado).HasMaxLength(30);
            entity.Property(p => p.Total).HasPrecision(10, 2);
        });

        modelBuilder.Entity<ItemPedido>(entity =>
        {
            entity.HasOne(i => i.Pedido)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Pizza)
                .WithMany()
                .HasForeignKey(i => i.PizzaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(i => i.PrecioUnitario).HasPrecision(10, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
