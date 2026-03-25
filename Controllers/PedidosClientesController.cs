using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosClientesController : ControllerBase
    {
        private const string EstadoEnPreparacion = "En preparación";
        private const string EstadoEntregado = "Entregado";
        private const string EstadoCancelado = "Cancelado";

        private readonly AppDbContext _context;
        private readonly InventarioAPI.Services.InventoryService _inventoryService;

        public PedidosClientesController(AppDbContext context, InventarioAPI.Services.InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoClienteDto>>> GetPedidosClientes([FromQuery] string? estado)
        {
            var query = _context.PedidosClientes
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var estadoNormalizado = estado.Trim().ToLower();
                query = query.Where(p => p.Estado.ToLower() == estadoNormalizado);
            }

            var pedidos = await query.ToListAsync();
            return pedidos.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoClienteDto>> GetPedidoCliente(int id)
        {
            var pedido = await _context.PedidosClientes
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return ToDto(pedido);
        }

        [HttpPost]
        public async Task<ActionResult<PedidoClienteDto>> PostPedidoCliente([FromBody] PedidoClienteCreateDto dto)
        {
            if (dto.Detalles == null || dto.Detalles.Count == 0)
            {
                return BadRequest("El pedido debe incluir al menos un detalle.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var pedidoTimestamp = dto.Timestamp ?? DateTime.UtcNow;

            var pedido = new PedidoCliente
            {
                ClienteId = dto.ClienteId,
                Fecha = int.Parse(pedidoTimestamp.ToString("yyyyMMdd")),
                Estado = EstadoEnPreparacion,
                Timestamp = pedidoTimestamp
            };

            _context.PedidosClientes.Add(pedido);
            await _context.SaveChangesAsync();

            var productoIds = dto.Detalles.Select(d => d.ProductoId).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var productosFaltantes = productoIds.Where(id => !productos.ContainsKey(id)).ToList();
            if (productosFaltantes.Count > 0)
            {
                return BadRequest($"Productos no encontrados: {string.Join(", ", productosFaltantes)}.");
            }

            var detalles = dto.Detalles.Select(d => new DetallePedido
            {
                PedidoId = pedido.Id,
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = productos[d.ProductoId].PrecioVenta,
                Timestamp = d.Timestamp ?? DateTime.UtcNow
            }).ToList();

            _context.DetallesPedidos.AddRange(detalles);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            pedido.Detalles = detalles;
            foreach (var d in pedido.Detalles)
            {
                d.Producto = productos[d.ProductoId];
            }

            return CreatedAtAction(nameof(GetPedidoCliente), new { id = pedido.Id }, ToDto(pedido));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedidoCliente(int id, [FromBody] PedidoClienteUpdateDto dto)
        {
            var pedido = await _context.PedidosClientes
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            if (!EsEnPreparacion(pedido.Estado) && dto.Detalles.Count > 0)
            {
                return BadRequest("Solo se pueden modificar detalles de pedidos en preparación.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Estado) && !EsEnPreparacion(dto.Estado))
            {
                return BadRequest("El estado solo puede cambiarse con /confirmar (entregado) o /cancelar.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var pedidoTimestamp = dto.Timestamp ?? DateTime.UtcNow;

            pedido.ClienteId = dto.ClienteId;
            pedido.Fecha = int.Parse(pedidoTimestamp.ToString("yyyyMMdd"));
            pedido.Estado = EstadoEnPreparacion;
            pedido.Timestamp = pedidoTimestamp;

            if (dto.Detalles.Count > 0)
            {
                _context.DetallesPedidos.RemoveRange(pedido.Detalles);

                var productoIds = dto.Detalles.Select(d => d.ProductoId).Distinct().ToList();
                var productos = await _context.Productos
                    .Where(p => productoIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                var productosFaltantes = productoIds.Where(pid => !productos.ContainsKey(pid)).ToList();
                if (productosFaltantes.Count > 0)
                {
                    return BadRequest($"Productos no encontrados: {string.Join(", ", productosFaltantes)}.");
                }

                var nuevosDetalles = dto.Detalles.Select(d => new DetallePedido
                {
                    PedidoId = pedido.Id,
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = productos[d.ProductoId].PrecioVenta,
                    Timestamp = d.Timestamp ?? DateTime.UtcNow
                }).ToList();

                _context.DetallesPedidos.AddRange(nuevosDetalles);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedidoCliente(int id)
        {
            var pedido = await _context.PedidosClientes.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            _context.PedidosClientes.Remove(pedido);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/confirmar")]
        public async Task<IActionResult> ConfirmarPedido(int id)
        {
            var pedido = await _context.PedidosClientes
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            if (pedido.Estado.Equals(EstadoEntregado, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Pedido ya entregado.");
            }

            if (pedido.Estado.Equals(EstadoCancelado, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("No se puede entregar un pedido cancelado.");
            }

            if (!EsEnPreparacion(pedido.Estado))
            {
                return BadRequest("Solo pedidos en preparación pueden entregarse.");
            }

            // Aggregate required quantities by producto to validate availability
            var requeridos = pedido.Detalles
                .GroupBy(d => d.ProductoId)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

            // Validate availability before making changes
            foreach (var kv in requeridos)
            {
                var producto = await _context.Productos.FindAsync(kv.Key);
                if (producto == null)
                {
                    return BadRequest($"Producto {kv.Key} no encontrado.");
                }

                if (producto.StockActual < kv.Value)
                {
                    return BadRequest($"Stock insuficiente para el producto {producto.Codigo} ({producto.Nombre}). Disponible: {producto.StockActual}, requerido: {kv.Value}.");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Decrement stock per detalle
            foreach (var detalle in pedido.Detalles)
            {
                var res = await _inventoryService.DecreaseStockAsync(detalle.ProductoId, detalle.Cantidad, detalle.Timestamp);
                if (!res.Success)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(res.Error);
                }
            }

            pedido.Estado = EstadoEntregado;
            pedido.Timestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return NoContent();
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarPedido(int id)
        {
            var pedido = await _context.PedidosClientes
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            if (pedido.Estado.Equals(EstadoCancelado, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("El pedido ya está cancelado.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (pedido.Estado.Equals(EstadoEntregado, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var detalle in pedido.Detalles)
                {
                    var res = await _inventoryService.IncreaseStockAsync(detalle.ProductoId, detalle.Cantidad, DateTime.UtcNow);
                    if (!res.Success)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(res.Error);
                    }
                }
            }

            pedido.Estado = EstadoCancelado;
            pedido.Timestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return NoContent();
        }

        private static bool EsEnPreparacion(string estado)
        {
            return estado.Equals("En preparación", StringComparison.OrdinalIgnoreCase)
                || estado.Equals("En preparacion", StringComparison.OrdinalIgnoreCase);
        }

        private static PedidoClienteDto ToDto(PedidoCliente pedido)
        {
            var detalles = pedido.Detalles.Select(d => new PedidoClienteDetalleDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                NombreProducto = d.Producto?.Nombre ?? string.Empty,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario,
                Timestamp = d.Timestamp
            }).ToList();

            return new PedidoClienteDto
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                Estado = pedido.Estado,
                Timestamp = pedido.Timestamp,
                TotalVenta = detalles.Sum(d => d.Subtotal),
                Detalles = detalles
            };
        }
    }
}
