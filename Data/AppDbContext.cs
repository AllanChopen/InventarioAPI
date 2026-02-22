using InventarioAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Movimiento> Movimientos => Set<Movimiento>();
        public DbSet<OrdenCompra> OrdenesCompras => Set<OrdenCompra>();
        public DbSet<DetalleOrdenCompra> DetallesOrdenesCompras => Set<DetalleOrdenCompra>();
        public DbSet<PedidoCliente> PedidosClientes => Set<PedidoCliente>();
        public DbSet<DetallePedido> DetallesPedidos => Set<DetallePedido>();
    }
}
