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
            var pedidos = await _context.PedidosClientes.ToListAsync();
            return pedidos.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoClienteDto>> GetPedidoCliente(int id)
        {
            var pedido = await _context.PedidosClientes.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            return ToDto(pedido);
        }

        [HttpPost]
        public async Task<ActionResult<PedidoClienteDto>> PostPedidoCliente([FromBody] PedidoClienteCreateDto dto)
        {
            var pedido = new PedidoCliente
            {
                ClienteId = dto.ClienteId,
                Fecha = dto.Fecha,
                Estado = dto.Estado,
                Timestamp = dto.Timestamp
            };

            _context.PedidosClientes.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPedidoCliente), new { id = pedido.Id }, ToDto(pedido));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedidoCliente(int id, [FromBody] PedidoClienteUpdateDto dto)
        {
            var pedido = await _context.PedidosClientes.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            pedido.ClienteId = dto.ClienteId;
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
            return new PedidoClienteDto
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                Fecha = pedido.Fecha,
                Estado = pedido.Estado,
                Timestamp = pedido.Timestamp
            };
        }
    }
}
