using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovimientosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetMovimientos()
        {
            var movimientos = await _context.Movimientos.ToListAsync();
            return movimientos.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovimientoDto>> GetMovimiento(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null)
            {
                return NotFound();
            }

            return ToDto(movimiento);
        }

        [HttpPost]
        public async Task<ActionResult<MovimientoDto>> PostMovimiento([FromBody] MovimientoCreateDto dto)
        {
            var movimiento = new Movimiento
            {
                ProductoId = dto.ProductoId,
                Tipo = dto.Tipo,
                Cantidad = dto.Cantidad,
                Timestamp = dto.Timestamp
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMovimiento), new { id = movimiento.Id }, ToDto(movimiento));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovimiento(int id, [FromBody] MovimientoUpdateDto dto)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null)
            {
                return NotFound();
            }

            movimiento.ProductoId = dto.ProductoId;
            movimiento.Tipo = dto.Tipo;
            movimiento.Cantidad = dto.Cantidad;
            movimiento.Timestamp = dto.Timestamp;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovimiento(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null)
            {
                return NotFound();
            }

            _context.Movimientos.Remove(movimiento);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static MovimientoDto ToDto(Movimiento movimiento)
        {
            return new MovimientoDto
            {
                Id = movimiento.Id,
                ProductoId = movimiento.ProductoId,
                Tipo = movimiento.Tipo,
                Cantidad = movimiento.Cantidad,
                Timestamp = movimiento.Timestamp
            };
        }
    }
}
