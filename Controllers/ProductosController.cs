using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            var productos = await _context.Productos.ToListAsync();
            return productos.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            return ToDto(producto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductoDto>> PostProducto([FromBody] ProductoCreateDto dto)
        {
            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                PrecioVenta = dto.PrecioVenta,
                CostoUnitario = dto.CostoUnitario,
                StockActual = dto.StockActual,
                StockMinimo = dto.StockMinimo,
                Estado = dto.Estado,
                CreadoPorId = dto.CreadoPorId,
                Timestamp = dto.Timestamp
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, ToDto(producto));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, [FromBody] ProductoUpdateDto dto)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.PrecioVenta = dto.PrecioVenta;
            producto.CostoUnitario = dto.CostoUnitario;
            producto.StockActual = dto.StockActual;
            producto.StockMinimo = dto.StockMinimo;
            producto.Estado = dto.Estado;
            producto.CreadoPorId = dto.CreadoPorId;
            producto.Timestamp = dto.Timestamp;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static ProductoDto ToDto(Producto producto)
        {
            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioVenta = producto.PrecioVenta,
                CostoUnitario = producto.CostoUnitario,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                Estado = producto.Estado,
                CreadoPorId = producto.CreadoPorId,
                Timestamp = producto.Timestamp
            };
        }
    }
}
