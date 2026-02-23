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
            var detalles = await _context.DetallesPedidos.ToListAsync();
            return detalles.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetallePedidoDto>> GetDetallePedido(int id)
        {
            var detalle = await _context.DetallesPedidos.FindAsync(id);
            if (detalle == null)
            {
                return NotFound();
            }

            return ToDto(detalle);
        }

        [HttpPost]
        public async Task<ActionResult<DetallePedidoDto>> PostDetallePedido([FromBody] DetallePedidoCreateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var decrease = await _inventoryService.DecreaseStockAsync(dto.ProductoId, dto.Cantidad, dto.Timestamp);
            if (!decrease.Success)
            {
                return BadRequest(decrease.Error);
            }

            var detalle = new DetallePedido
            {
                PedidoId = dto.PedidoId,
                ProductoId = dto.ProductoId,
                Cantidad = dto.Cantidad,
                PrecioUnitario = dto.PrecioUnitario,
                Timestamp = dto.Timestamp
            };

            _context.DetallesPedidos.Add(detalle);
            await _context.SaveChangesAsync();

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
                Timestamp = detalle.Timestamp
            };
        }
    }
}
