using APIrJordan.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace APIrJordan.Web.Data;

/// <summary>Conexión a MySQL y mapeo de las cinco tablas del sistema.</summary>
public class ApirJordanDbContext : DbContext
{
    public ApirJordanDbContext(DbContextOptions<ApirJordanDbContext> opciones) : base(opciones)
    {
    }

    public DbSet<Zapatilla> Zapatillas => Set<Zapatilla>();
    public DbSet<StockTalle> StockTalles => Set<StockTalle>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItemsPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Zapatilla>(entity =>
        {
            entity.Property(z => z.Nombre).HasMaxLength(120);
            entity.Property(z => z.Colorway).HasMaxLength(80);
            entity.Property(z => z.Descripcion).HasMaxLength(600);
            entity.Property(z => z.Precio).HasPrecision(10, 2);
            entity.Property(z => z.Categoria).HasMaxLength(30);
            entity.Property(z => z.Imagen).HasMaxLength(120);
            entity.HasIndex(z => z.Categoria);
        });

        modelBuilder.Entity<StockTalle>(entity =>
        {
            entity.HasOne(s => s.Zapatilla)
                .WithMany(z => z.Talles)
                .HasForeignKey(s => s.ZapatillaId)
                .OnDelete(DeleteBehavior.Cascade);      // si se borra el modelo, se borra su stock

            entity.Property(s => s.Talle).HasPrecision(3, 1);

            // Token de concurrencia: el UPDATE lleva "WHERE Cantidad = valor leído".
            // Si otro pedido cambió el stock en el medio, EF lanza DbUpdateConcurrencyException.
            entity.Property(s => s.Cantidad).IsConcurrencyToken();

            // Un modelo no puede tener dos filas para el mismo talle.
            entity.HasIndex(s => new { s.ZapatillaId, s.Talle }).IsUnique();
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
                .OnDelete(DeleteBehavior.Restrict);     // no se puede borrar un cliente con pedidos

            entity.Property(p => p.Estado).HasMaxLength(30);
            entity.Property(p => p.Total).HasPrecision(10, 2);
            entity.Property(p => p.Nota).HasMaxLength(300);
            entity.HasIndex(p => p.Estado);
        });

        modelBuilder.Entity<ItemPedido>(entity =>
        {
            entity.HasOne(i => i.Pedido)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);      // las líneas no tienen sentido sin su pedido

            entity.HasOne(i => i.Zapatilla)
                .WithMany()
                .HasForeignKey(i => i.ZapatillaId)
                .OnDelete(DeleteBehavior.Restrict);     // no se puede borrar un modelo que ya se vendió

            entity.Property(i => i.Talle).HasPrecision(3, 1);
            entity.Property(i => i.PrecioUnitario).HasPrecision(10, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
