using InventarioAPI.Data;
using InventarioAPI.DTOs;
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
        public async Task<ActionResult<IEnumerable<OrdenCompraDto>>> GetOrdenesCompras()
        {
            var ordenes = await _context.OrdenesCompras.ToListAsync();
            return ordenes.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenCompraDto>> GetOrdenCompra(int id)
        {
            var orden = await _context.OrdenesCompras.FindAsync(id);
            if (orden == null)
            {
                return NotFound();
            }

            return ToDto(orden);
        }

        [HttpPost]
        public async Task<ActionResult<OrdenCompraDto>> PostOrdenCompra([FromBody] OrdenCompraCreateDto dto)
        {
            var orden = new OrdenCompra
            {
                ProveedorId = dto.ProveedorId,
                Fecha = dto.Fecha,
                Estado = dto.Estado,
                Timestamp = dto.Timestamp
            };

            _context.OrdenesCompras.Add(orden);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrdenCompra), new { id = orden.Id }, ToDto(orden));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrdenCompra(int id, [FromBody] OrdenCompraUpdateDto dto)
        {
            var orden = await _context.OrdenesCompras.FindAsync(id);
            if (orden == null)
            {
                return NotFound();
            }

            orden.ProveedorId = dto.ProveedorId;
            orden.Fecha = dto.Fecha;
            orden.Estado = dto.Estado;
            orden.Timestamp = dto.Timestamp;

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

        private static OrdenCompraDto ToDto(OrdenCompra orden)
        {
            return new OrdenCompraDto
            {
                Id = orden.Id,
                ProveedorId = orden.ProveedorId,
                Fecha = orden.Fecha,
                Estado = orden.Estado,
                Timestamp = orden.Timestamp
            };
        }
    }
}
