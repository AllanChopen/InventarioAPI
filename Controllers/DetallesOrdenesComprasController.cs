using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetallesOrdenesComprasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetallesOrdenesComprasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleOrdenCompraDto>>> GetDetallesOrdenesCompras()
        {
            var detalles = await _context.DetallesOrdenesCompras.ToListAsync();
            return detalles.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleOrdenCompraDto>> GetDetalleOrdenCompra(int id)
        {
            var detalle = await _context.DetallesOrdenesCompras.FindAsync(id);
            if (detalle == null)
            {
                return NotFound();
            }

            return ToDto(detalle);
        }

        [HttpPost]
        public async Task<ActionResult<DetalleOrdenCompraDto>> PostDetalleOrdenCompra([FromBody] DetalleOrdenCompraCreateDto dto)
        {
            var detalle = new DetalleOrdenCompra
            {
                OrdenId = dto.OrdenId,
                ProductoId = dto.ProductoId,
                Cantidad = dto.Cantidad,
                CostoUnitario = dto.CostoUnitario,
                Timestamp = dto.Timestamp
            };

            _context.DetallesOrdenesCompras.Add(detalle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetalleOrdenCompra), new { id = detalle.Id }, ToDto(detalle));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalleOrdenCompra(int id, [FromBody] DetalleOrdenCompraUpdateDto dto)
        {
            var detalle = await _context.DetallesOrdenesCompras.FindAsync(id);
            if (detalle == null)
            {
                return NotFound();
            }

            detalle.OrdenId = dto.OrdenId;
            detalle.ProductoId = dto.ProductoId;
            detalle.Cantidad = dto.Cantidad;
            detalle.CostoUnitario = dto.CostoUnitario;
            detalle.Timestamp = dto.Timestamp;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalleOrdenCompra(int id)
        {
            var detalle = await _context.DetallesOrdenesCompras.FindAsync(id);
            if (detalle == null)
            {
                return NotFound();
            }

            _context.DetallesOrdenesCompras.Remove(detalle);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static DetalleOrdenCompraDto ToDto(DetalleOrdenCompra detalle)
        {
            return new DetalleOrdenCompraDto
            {
                Id = detalle.Id,
                OrdenId = detalle.OrdenId,
                ProductoId = detalle.ProductoId,
                Cantidad = detalle.Cantidad,
                CostoUnitario = detalle.CostoUnitario,
                Timestamp = detalle.Timestamp
            };
        }
    }
}
