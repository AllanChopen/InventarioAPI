using InventarioAPI.Data;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenesComprasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdenesComprasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdenCompra>>> GetOrdenesCompras()
        {
            return await _context.OrdenesCompras.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenCompra>> GetOrdenCompra(int id)
        {
            var orden = await _context.OrdenesCompras.FindAsync(id);
            if (orden == null)
            {
                return NotFound();
            }

            return orden;
        }

        [HttpPost]
        public async Task<ActionResult<OrdenCompra>> PostOrdenCompra(OrdenCompra ordenCompra)
        {
            _context.OrdenesCompras.Add(ordenCompra);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrdenCompra), new { id = ordenCompra.Id }, ordenCompra);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrdenCompra(int id, OrdenCompra ordenCompra)
        {
            if (id != ordenCompra.Id)
            {
                return BadRequest();
            }

            var exists = await _context.OrdenesCompras.AnyAsync(o => o.Id == id);
            if (!exists)
            {
                return NotFound();
            }

            _context.Entry(ordenCompra).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrdenCompra(int id)
        {
            var orden = await _context.OrdenesCompras.FindAsync(id);
            if (orden == null)
            {
                return NotFound();
            }

            _context.OrdenesCompras.Remove(orden);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
