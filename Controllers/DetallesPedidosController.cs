using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetallesPedidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetallesPedidosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetallePedidoDto>>> GetDetallesPedidos()
        {
            var detalles = await _context.DetallesPedidos
                .Include(d => d.Producto)
                .ToListAsync();
            return detalles.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetallePedidoDto>> GetDetallePedido(int id)
        {
            var detalle = await _context.DetallesPedidos
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (detalle == null)
            {
                return NotFound();
            }

            return ToDto(detalle);
        }

        [HttpPost]
        public async Task<ActionResult<DetallePedidoDto>> PostDetallePedido([FromBody] DetallePedidoCreateDto dto)
        {
            var pedido = await _context.PedidosClientes.FindAsync(dto.PedidoId);
            if (pedido == null)
            {
                return BadRequest("Pedido no encontrado.");
            }

            if (!pedido.Estado.Equals("En preparación", StringComparison.OrdinalIgnoreCase)
                && !pedido.Estado.Equals("En preparacion", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Solo se pueden agregar detalles a pedidos en preparación.");
            }

            var producto = await _context.Productos.FindAsync(dto.ProductoId);
            if (producto == null)
            {
                return BadRequest("Producto no encontrado.");
            }

            var detalle = new DetallePedido
            {
                PedidoId = dto.PedidoId,
                ProductoId = dto.ProductoId,
                Cantidad = dto.Cantidad,
                PrecioUnitario = producto.PrecioVenta,
                Timestamp = dto.Timestamp
            };

            _context.DetallesPedidos.Add(detalle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetallePedido), new { id = detalle.Id }, ToDto(detalle));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetallePedido(int id, [FromBody] DetallePedidoUpdateDto dto)
        {
            var detalle = await _context.DetallesPedidos.FindAsync(id);
            if (detalle == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(dto.ProductoId);
            if (producto == null)
            {
                return BadRequest("Producto no encontrado.");
            }

            detalle.PedidoId = dto.PedidoId;
            detalle.ProductoId = dto.ProductoId;
            detalle.Cantidad = dto.Cantidad;
            detalle.PrecioUnitario = producto.PrecioVenta;
            detalle.Timestamp = dto.Timestamp;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetallePedido(int id)
        {
            var detalle = await _context.DetallesPedidos.FindAsync(id);
            if (detalle == null)
            {
                return NotFound();
            }

            _context.DetallesPedidos.Remove(detalle);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static DetallePedidoDto ToDto(DetallePedido detalle)
        {
            return new DetallePedidoDto
            {
                Id = detalle.Id,
                PedidoId = detalle.PedidoId,
                ProductoId = detalle.ProductoId,
                NombreProducto = detalle.Producto?.Nombre ?? string.Empty,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Subtotal = detalle.Cantidad * detalle.PrecioUnitario,
                Timestamp = detalle.Timestamp
            };
        }
    }
}
