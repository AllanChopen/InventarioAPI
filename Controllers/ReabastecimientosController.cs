using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReabastecimientosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReabastecimientosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReabastecimientoDto>>> GetAll()
        {
            var items = await _context.Reabastecimientos
                .Include(r => r.Producto)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();

            return items.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReabastecimientoDto>> GetById(int id)
        {
            var item = await _context.Reabastecimientos
                .Include(r => r.Producto)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return ToDto(item);
        }

        [HttpPost]
        public async Task<ActionResult<ReabastecimientoDto>> Create([FromBody] ReabastecimientoCreateDto dto)
        {
            var producto = await _context.Productos.FindAsync(dto.ProductoId);
            if (producto == null)
            {
                return BadRequest("Producto no encontrado.");
            }

            var entity = new Reabastecimiento
            {
                ProductoId = dto.ProductoId,
                CantidadSugerida = dto.CantidadSugerida,
                Estado = "Pendiente",
                Timestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp
            };

            _context.Reabastecimientos.Add(entity);
            await _context.SaveChangesAsync();

            await _context.Entry(entity).Reference(e => e.Producto).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReabastecimientoUpdateDto dto)
        {
            var entity = await _context.Reabastecimientos.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.CantidadSugerida = dto.CantidadSugerida;
            entity.Estado = dto.Estado;
            entity.Timestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/aprobar")]
        public async Task<IActionResult> Aprobar(int id)
        {
            var entity = await _context.Reabastecimientos.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Estado = "Aprobada";
            entity.Timestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var entity = await _context.Reabastecimientos.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Estado = "Cancelada";
            entity.Timestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static ReabastecimientoDto ToDto(Reabastecimiento r)
        {
            return new ReabastecimientoDto
            {
                Id = r.Id,
                ProductoId = r.ProductoId,
                CodigoProducto = r.Producto?.Codigo ?? string.Empty,
                NombreProducto = r.Producto?.Nombre ?? string.Empty,
                CantidadSugerida = r.CantidadSugerida,
                Estado = r.Estado,
                Timestamp = r.Timestamp
            };
        }
    }
}
