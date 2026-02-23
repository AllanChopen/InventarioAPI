using InventarioAPI.Data;
using InventarioAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using InventarioAPI.Hubs;

namespace InventarioAPI.Services
{
    public class InventoryService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<InventoryHub> _hubContext;

        public InventoryService(AppDbContext context, IHubContext<InventoryHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<(bool Success, string? Error)> DecreaseStockAsync(int productoId, int cantidad, DateTime timestamp)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                return (false, "Producto no encontrado.");
            }

            if (cantidad <= 0)
            {
                return (false, "Cantidad debe ser mayor que cero.");
            }

            if (producto.StockActual - cantidad < 0)
            {
                return (false, "Stock insuficiente para registrar la salida.");
            }

            producto.StockActual -= cantidad;

            var movimiento = new Movimiento
            {
                ProductoId = productoId,
                Tipo = "Salida",
                Cantidad = cantidad,
                Timestamp = timestamp
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("StockChanged", new
            {
                ProductoId = producto.Id,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                StockBajo = producto.StockActual <= producto.StockMinimo
            });

            await CrearReabastecimientoSiCorresponde(producto);

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> IncreaseStockAsync(int productoId, int cantidad, DateTime timestamp)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                return (false, "Producto no encontrado.");
            }

            if (cantidad <= 0)
            {
                return (false, "Cantidad debe ser mayor que cero.");
            }

            producto.StockActual += cantidad;

            var movimiento = new Movimiento
            {
                ProductoId = productoId,
                Tipo = "Entrada",
                Cantidad = cantidad,
                Timestamp = timestamp
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("StockChanged", new
            {
                ProductoId = producto.Id,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                StockBajo = producto.StockActual <= producto.StockMinimo
            });

            return (true, null);
        }

        private async Task CrearReabastecimientoSiCorresponde(Producto producto)
        {
            if (producto.StockActual > producto.StockMinimo)
            {
                return;
            }

            var yaPendiente = await _context.Reabastecimientos
                .AnyAsync(r => r.ProductoId == producto.Id && r.Estado == "Pendiente");

            if (yaPendiente)
            {
                return;
            }

            var sugerida = CalcularCantidadSugerida(producto);

            var reabastecimiento = new Reabastecimiento
            {
                ProductoId = producto.Id,
                CantidadSugerida = sugerida,
                Estado = "Pendiente",
                Timestamp = DateTime.UtcNow
            };

            // Create reabastecimiento
            _context.Reabastecimientos.Add(reabastecimiento);
            await _context.SaveChangesAsync();

            // Determine a suggested proveedor for the reabastecimiento (may create a fallback proveedor)
            var proveedorId = await _context.Proveedores.Select(p => p.Id).FirstOrDefaultAsync();
            if (proveedorId == 0)
            {
                var proveedorFallback = new Proveedor
                {
                    Nombre = "Proveedor Sugerido",
                    Telefono = string.Empty,
                    Email = string.Empty,
                    Direccion = string.Empty,
                    Estado = true,
                    Timestamp = DateTime.UtcNow
                };

                _context.Proveedores.Add(proveedorFallback);
                await _context.SaveChangesAsync();
                proveedorId = proveedorFallback.Id;
            }

            // Attach suggested proveedor to the reabastecimiento but DO NOT create an OrdenCompra yet.
            reabastecimiento.ProveedorSugeridoId = proveedorId;
            _context.Reabastecimientos.Update(reabastecimiento);
            await _context.SaveChangesAsync();

            // Notify clients that a reabastecimiento was created (with suggested proveedor), manager will approve to create the order
            await _hubContext.Clients.All.SendAsync("ReabastecimientoCreated", new
            {
                ReabastecimientoId = reabastecimiento.Id,
                ProductoId = producto.Id,
                CantidadSugerida = reabastecimiento.CantidadSugerida,
                ProveedorSugeridoId = proveedorId
            });
        }

        private static int CalcularCantidadSugerida(Producto producto)
        {
            var objetivo = Math.Max(producto.StockMinimo * 2, producto.StockMinimo + 1);
            var sugerida = objetivo - producto.StockActual;
            return sugerida <= 0 ? producto.StockMinimo : sugerida;
        }
    }
}
