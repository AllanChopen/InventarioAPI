using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetProveedores([FromQuery] string? categoria)
        {
            var query = _context.Proveedores
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                var categoriaTerm = categoria.ToLower();
                query = query.Where(p => p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(categoriaTerm));
            }

            var proveedores = await query.ToListAsync();

            var productosPorCategoria = await GetProductosAsociadosPorCategoriaAsync(
                proveedores.Select(p => p.CategoriaId).Distinct().ToList());

            return proveedores.Select(p => ToDto(p, productosPorCategoria)).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProveedorDto>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (proveedor == null)
            {
                return NotFound();
            }

            var productosPorCategoria = await GetProductosAsociadosPorCategoriaAsync(new[] { proveedor.CategoriaId });
            return ToDto(proveedor, productosPorCategoria);
        }

        [HttpGet("nombre/{nombre}")]
        public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetProveedoresByNombre(string nombre)
        {
            var proveedores = await _context.Proveedores
                .Include(p => p.Categoria)
                .Where(p => p.Nombre.ToLower() == nombre.ToLower())
                .ToListAsync();

            var productosPorCategoria = await GetProductosAsociadosPorCategoriaAsync(
                proveedores.Select(p => p.CategoriaId).Distinct().ToList());

            return proveedores.Select(p => ToDto(p, productosPorCategoria)).ToList();
        }

        [HttpPost]
        public async Task<ActionResult<ProveedorDto>> PostProveedor([FromBody] ProveedorCreateDto dto)
        {
            if (!await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
            {
                return BadRequest("La categoria seleccionada no existe.");
            }

            var proveedor = new Proveedor
            {
                Nombre = dto.Nombre,
                CategoriaId = dto.CategoriaId,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion,
                Estado = dto.Estado,
                Timestamp = dto.Timestamp
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            await _context.Entry(proveedor)
                .Reference(p => p.Categoria)
                .LoadAsync();

            var productosPorCategoria = await GetProductosAsociadosPorCategoriaAsync(new[] { proveedor.CategoriaId });
            return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, ToDto(proveedor, productosPorCategoria));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProveedor(int id, [FromBody] ProveedorUpdateDto dto)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }

            if (!await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
            {
                return BadRequest("La categoria seleccionada no existe.");
            }

            proveedor.Nombre = dto.Nombre;
            proveedor.CategoriaId = dto.CategoriaId;
            proveedor.Telefono = dto.Telefono;
            proveedor.Email = dto.Email;
            proveedor.Direccion = dto.Direccion;
            proveedor.Estado = dto.Estado;
            proveedor.Timestamp = dto.Timestamp;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<Dictionary<int, List<ProveedorProductoDto>>> GetProductosAsociadosPorCategoriaAsync(IEnumerable<int> categoriaIds)
        {
            var categoriaIdsList = categoriaIds.Distinct().ToList();
            if (categoriaIdsList.Count == 0)
            {
                return new Dictionary<int, List<ProveedorProductoDto>>();
            }

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Bodega)
                .Where(p => categoriaIdsList.Contains(p.CategoriaId))
                .OrderBy(p => p.Nombre)
                .Select(p => new ProveedorProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Codigo = p.Codigo,
                    CategoriaId = p.CategoriaId,
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : string.Empty,
                    BodegaId = p.BodegaId,
                    BodegaNombre = p.Bodega != null ? p.Bodega.Nombre : string.Empty,
                    StockActual = p.StockActual,
                    StockMinimo = p.StockMinimo
                })
                .ToListAsync();

            return productos
                .GroupBy(p => p.CategoriaId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static ProveedorDto ToDto(Proveedor proveedor, IReadOnlyDictionary<int, List<ProveedorProductoDto>> productosPorCategoria)
        {
            return new ProveedorDto
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                CategoriaId = proveedor.CategoriaId,
                Categoria = proveedor.Categoria?.Nombre ?? string.Empty,
                Productos = productosPorCategoria.TryGetValue(proveedor.CategoriaId, out var productos)
                    ? productos
                    : new List<ProveedorProductoDto>(),
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Direccion = proveedor.Direccion,
                Estado = proveedor.Estado,
                Timestamp = proveedor.Timestamp
            };
        }
    }
}
