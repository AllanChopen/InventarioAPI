using InventarioAPI.Data;
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
        public async Task<ActionResult<IEnumerable<DetalleOrdenCompra>>> GetDetallesOrdenesCompras()
        {
            return await _context.DetallesOrdenesCompras.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleOrdenCompra>> GetDetalleOrdenCompra(int id)
        {
            var detalle = await _context.DetallesOrdenesCompras.FindAsync(id);
            if (detalle == null)
            {
                return NotFound();
            }

            return detalle;
        }

        [HttpPost]
        public async Task<ActionResult<DetalleOrdenCompra>> PostDetalleOrdenCompra(DetalleOrdenCompra detalleOrdenCompra)
        {
            _context.DetallesOrdenesCompras.Add(detalleOrdenCompra);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDetalleOrdenCompra), new { id = detalleOrdenCompra.Id }, detalleOrdenCompra);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalleOrdenCompra(int id, DetalleOrdenCompra detalleOrdenCompra)
        {
            if (id != detalleOrdenCompra.Id)
            {
                return BadRequest();
            }

            var exists = await _context.DetallesOrdenesCompras.AnyAsync(d => d.Id == id);
            if (!exists)
            {
                return NotFound();
            }

            _context.Entry(detalleOrdenCompra).State = EntityState.Modified;
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
    }
}
