using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using InventarioAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly InventoryService _inventoryService;

        public MovimientosController(AppDbContext context, InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
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
            if (!IsTipoValido(dto.Tipo))
            {
                return BadRequest("Tipo de movimiento debe ser 'Entrada' o 'Salida'.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (dto.Tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _inventoryService.IncreaseStockAsync(dto.ProductoId, dto.Cantidad, dto.Timestamp);
                if (!result.Success) return BadRequest(result.Error);
            }
            else // Salida
            {
                var result = await _inventoryService.DecreaseStockAsync(dto.ProductoId, dto.Cantidad, dto.Timestamp);
                if (!result.Success) return BadRequest(result.Error);
            }

            // Load the movimiento we just created to return it
            var movimiento = await _context.Movimientos
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync(m => m.ProductoId == dto.ProductoId && m.Timestamp == dto.Timestamp && m.Cantidad == dto.Cantidad && m.Tipo == dto.Tipo);

            await transaction.CommitAsync();

            if (movimiento == null)
            {
                return StatusCode(500, "No se pudo crear el movimiento.");
            }

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

            if (!IsTipoValido(dto.Tipo))
            {
                return BadRequest("Tipo de movimiento debe ser 'Entrada' o 'Salida'.");
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

        private static bool IsTipoValido(string tipo)
        {
            return tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase) ||
                   tipo.Equals("Salida", StringComparison.OrdinalIgnoreCase);
        }

        // Reabastecimiento logic moved to InventoryService to avoid duplication
    }
}
