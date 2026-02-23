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
            if (!IsTipoValido(dto.Tipo))
            {
                return BadRequest("Tipo de movimiento debe ser 'Entrada' o 'Salida'.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var producto = await _context.Productos.FindAsync(dto.ProductoId);
            if (producto == null)
            {
                return BadRequest("Producto no encontrado.");
            }

            var movimiento = new Movimiento
            {
                ProductoId = dto.ProductoId,
                Tipo = dto.Tipo,
                Cantidad = dto.Cantidad,
                Timestamp = dto.Timestamp
            };

            if (dto.Tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase))
            {
                producto.StockActual += dto.Cantidad;
            }
            else if (dto.Tipo.Equals("Salida", StringComparison.OrdinalIgnoreCase))
            {
                if (producto.StockActual - dto.Cantidad < 0)
                {
                    return BadRequest("Stock insuficiente para registrar la salida.");
                }

                producto.StockActual -= dto.Cantidad;
            }

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            // Trigger auto-replenishment suggestion if needed
            await CrearReabastecimientoSiCorresponde(producto);

            await transaction.CommitAsync();

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

        private async Task CrearReabastecimientoSiCorresponde(Producto producto)
        {
            if (producto.StockActual > producto.StockMinimo)
            {
                return;
            }

            var yaPendiente = await _context.Reabastecimientos
                .AnyAsync(r => r.ProductoId == producto.Id && r.Estado == "Pendiente");

            if (yaPendiente)
            {
                return;
            }

            var sugerida = CalcularCantidadSugerida(producto);

            var reabastecimiento = new Reabastecimiento
            {
                ProductoId = producto.Id,
                CantidadSugerida = sugerida,
                Estado = "Pendiente",
                Timestamp = DateTime.UtcNow
            };

            _context.Reabastecimientos.Add(reabastecimiento);
            await _context.SaveChangesAsync();
        }

        private static int CalcularCantidadSugerida(Producto producto)
        {
            var objetivo = Math.Max(producto.StockMinimo * 2, producto.StockMinimo + 1);
            var sugerida = objetivo - producto.StockActual;
            return sugerida <= 0 ? producto.StockMinimo : sugerida;
        }
    }
}
