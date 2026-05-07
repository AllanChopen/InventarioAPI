using InventarioAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Bodega> Bodegas => Set<Bodega>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Movimiento> Movimientos => Set<Movimiento>();
        public DbSet<OrdenCompra> OrdenesCompras => Set<OrdenCompra>();
        public DbSet<DetalleOrdenCompra> DetallesOrdenesCompras => Set<DetalleOrdenCompra>();
        public DbSet<PedidoCliente> PedidosClientes => Set<PedidoCliente>();
        public DbSet<DetallePedido> DetallesPedidos => Set<DetallePedido>();
        public DbSet<Reabastecimiento> Reabastecimientos => Set<Reabastecimiento>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto>()
                .Property(p => p.BodegaId)
                .HasDefaultValue(1);

            modelBuilder.Entity<Producto>()
                .Property(p => p.CategoriaId)
                .HasDefaultValue(1);

            modelBuilder.Entity<Proveedor>()
                .Property(p => p.CategoriaId)
                .HasDefaultValue(1);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Bodega)
                .WithMany(b => b.Productos)
                .HasForeignKey(p => p.BodegaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Proveedor>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Proveedores)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bodega>().HasData(
                new Bodega { Id = 1, Nombre = "Bodega Central" },
                new Bodega { Id = 2, Nombre = "Bodega Sur" },
                new Bodega { Id = 3, Nombre = "Bodega Norte" }
            );

            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Hardware" },
                new Categoria { Id = 2, Nombre = "Periféricos" },
                new Categoria { Id = 3, Nombre = "Audio" },
                new Categoria { Id = 4, Nombre = "Monitores" }
            );
        }
    }
}
