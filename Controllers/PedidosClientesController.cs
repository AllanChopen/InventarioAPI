using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly AppDbContext _context;
        private readonly InventarioAPI.Services.InventoryService _inventoryService;

        public PedidosClientesController(AppDbContext context, InventarioAPI.Services.InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoClienteDto>>> GetPedidosClientes()
        {
            var pedidos = await _context.PedidosClientes
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();

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
                return BadRequest("El pedido debe incluir al menos un producto.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var pedidoTimestamp = DateTime.UtcNow;

            var pedido = new PedidoCliente
            {
                Fecha = (int)new DateTimeOffset(pedidoTimestamp).ToUnixTimeSeconds(),
                Estado = "Pendiente",
                Timestamp = pedidoTimestamp
            };

            _context.PedidosClientes.Add(pedido);
            await _context.SaveChangesAsync();

            decimal total = 0m;
            foreach (var d in dto.Detalles ?? Enumerable.Empty<DetallePedidoCreateDto>())
            {
                if (d.ProductoId <= 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Cada detalle debe incluir un producto valido.");
                }

                if (d.Cantidad <= 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("La cantidad de cada producto debe ser mayor a cero.");
                }

                var producto = await _context.Productos.FindAsync(d.ProductoId);
                if (producto == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest($"Producto {d.ProductoId} no encontrado.");
                }

                var detalle = new DetallePedido
                {
                    PedidoId = pedido.Id,
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    Timestamp = DateTime.UtcNow
                };

                _context.DetallesPedidos.Add(detalle);
                total += d.Cantidad * producto.PrecioVenta;
            }

            await _context.SaveChangesAsync();

            pedido.Timestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var pedidoCreado = await _context.PedidosClientes
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstAsync(p => p.Id == pedido.Id);

            return CreatedAtAction(nameof(GetPedidoCliente), new { id = pedido.Id }, ToDto(pedidoCreado));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedidoCliente(int id, [FromBody] PedidoClienteUpdateDto dto)
        {
            var pedido = await _context.PedidosClientes.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            pedido.Fecha = dto.Fecha;
            pedido.Estado = dto.Estado;
            pedido.Timestamp = dto.Timestamp;

            await _context.SaveChangesAsync();
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

            if (pedido.Estado.Equals("Confirmado", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Pedido ya confirmado.");
            }

            var requeridos = pedido.Detalles
                .GroupBy(d => d.ProductoId)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

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

            foreach (var detalle in pedido.Detalles)
            {
                var res = await _inventoryService.DecreaseStockAsync(detalle.ProductoId, detalle.Cantidad, detalle.Timestamp);
                if (!res.Success)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(res.Error);
                }
            }

            pedido.Estado = "Confirmado";
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

            if (!pedido.Estado.Equals("Confirmado", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Solo pedidos confirmados pueden cancelarse.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var detalle in pedido.Detalles)
            {
                var res = await _inventoryService.IncreaseStockAsync(detalle.ProductoId, detalle.Cantidad, DateTime.UtcNow);
                if (!res.Success)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(res.Error);
                }
            }

            pedido.Estado = "Cancelado";
            pedido.Timestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return NoContent();
        }

        private static PedidoClienteDto ToDto(PedidoCliente pedido)
        {
            var detalles = pedido.Detalles?.Select(d => new DetallePedidoDto
            {
                Id = d.Id,
                PedidoId = d.PedidoId,
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Timestamp = d.Timestamp,
                Nombre = d.Producto?.Nombre ?? string.Empty,
                Codigo = d.Producto?.Codigo ?? string.Empty,
                Descripcion = d.Producto?.Descripcion ?? string.Empty,
                Total = d.Cantidad * d.PrecioUnitario
            }).ToList() ?? new List<DetallePedidoDto>();

            var total = detalles.Sum(x => x.Total);

            return new PedidoClienteDto
            {
                Id = pedido.Id,
                Fecha = pedido.Fecha,
                Estado = pedido.Estado,
                Timestamp = pedido.Timestamp,
                // Cliente removed
                Total = total,
                Detalles = detalles
            };
        }
    }
}
