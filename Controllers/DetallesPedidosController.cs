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
        private readonly InventarioAPI.Services.InventoryService _inventoryService;

        public DetallesPedidosController(AppDbContext context, InventarioAPI.Services.InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
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
            // For standalone detalle endpoint, PedidoId is required.
            if (!dto.PedidoId.HasValue)
            {
                return BadRequest("PedidoId es requerido para crear un detalle de pedido de forma independiente.");
            }

            var pedido = await _context.PedidosClientes.FindAsync(dto.PedidoId.Value);
            if (pedido == null)
            {
                return NotFound();
            }

            if (pedido.Estado.Equals("Cancelado", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("No se pueden agregar productos a un pedido cancelado.");
            }

            var producto = await _context.Productos.FindAsync(dto.ProductoId);
            if (producto == null)
            {
                return BadRequest($"Producto {dto.ProductoId} no encontrado.");
            }

            var descontarStock = pedido.Estado.Equals("Confirmado", StringComparison.OrdinalIgnoreCase);

            if (descontarStock && producto.StockActual < dto.Cantidad)
            {
                return BadRequest($"Stock insuficiente para el producto {producto.Codigo} ({producto.Nombre}). Disponible: {producto.StockActual}, requerido: {dto.Cantidad}.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (descontarStock)
            {
                var decrease = await _inventoryService.DecreaseStockAsync(dto.ProductoId, dto.Cantidad, DateTime.UtcNow);
                if (!decrease.Success)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(decrease.Error);
                }
            }

            var detalle = new DetallePedido
            {
                PedidoId = dto.PedidoId.Value,
                ProductoId = dto.ProductoId,
                Cantidad = dto.Cantidad,
                PrecioUnitario = producto.PrecioVenta,
                Timestamp = DateTime.UtcNow
            };

            _context.DetallesPedidos.Add(detalle);
            await _context.SaveChangesAsync();

            await _context.Entry(detalle)
                .Reference(d => d.Producto)
                .LoadAsync();

            await transaction.CommitAsync();

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

            detalle.PedidoId = dto.PedidoId;
            detalle.ProductoId = dto.ProductoId;
            detalle.Cantidad = dto.Cantidad;
            detalle.PrecioUnitario = dto.PrecioUnitario;
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
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Timestamp = detalle.Timestamp,
                Nombre = detalle.Producto?.Nombre ?? string.Empty,
                Codigo = detalle.Producto?.Codigo ?? string.Empty,
                Descripcion = detalle.Producto?.Descripcion ?? string.Empty,
                Total = detalle.Cantidad * detalle.PrecioUnitario
            };
        }
    }
}
