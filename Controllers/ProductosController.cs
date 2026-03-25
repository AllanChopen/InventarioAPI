using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioAPI.Services;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly InventoryService _inventoryService;

        public ProductosController(AppDbContext context, InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos(
            [FromQuery] string? search,
            [FromQuery] string? categoria,
            [FromQuery] string? ubicacion,
            [FromQuery] bool? soloBajoStock)
        {
            var query = ApplyProductosFilters(_context.Productos.AsQueryable(), search, categoria, ubicacion, soloBajoStock);

            var productos = await query.ToListAsync();
            return productos.Select(ToDto).ToList();
        }

        [HttpGet("resumen")]
        public async Task<ActionResult<ProductosResumenDto>> GetResumenProductos(
            [FromQuery] string? search,
            [FromQuery] string? categoria,
            [FromQuery] string? ubicacion,
            [FromQuery] bool? soloBajoStock)
        {
            var query = ApplyProductosFilters(_context.Productos.AsQueryable(), search, categoria, ubicacion, soloBajoStock);

            var resumen = new ProductosResumenDto
            {
                TotalProductos = await query.CountAsync(),
                UnidadesTotales = await query.SumAsync(p => (int?)p.StockActual) ?? 0,
                ProductosBajoStock = await query.CountAsync(p => p.StockActual <= p.StockMinimo),
                ValorInventarioCosto = await query.SumAsync(p => (decimal?)p.StockActual * p.CostoUnitario) ?? 0,
                ValorInventarioVenta = await query.SumAsync(p => (decimal?)p.StockActual * p.PrecioVenta) ?? 0,
                GeneratedAt = DateTime.UtcNow
            };

            return resumen;
        }

        [HttpGet("mas-vendidos")]
        public async Task<ActionResult<IEnumerable<ProductoMasVendidoDto>>> GetProductosMasVendidos(
            [FromQuery] int top = 10,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            if (top <= 0)
            {
                return BadRequest("El parámetro top debe ser mayor a 0.");
            }

            if (top > 100)
            {
                top = 100;
            }

            var query = from d in _context.DetallesPedidos
                        join p in _context.PedidosClientes on d.PedidoId equals p.Id
                        join pr in _context.Productos on d.ProductoId equals pr.Id
                        where p.Estado.ToLower() == "entregado"
                        select new { Detalle = d, Pedido = p, Producto = pr };

            if (desde.HasValue)
            {
                var desdeUtc = desde.Value.ToUniversalTime();
                query = query.Where(x => x.Pedido.Timestamp >= desdeUtc);
            }

            if (hasta.HasValue)
            {
                var hastaUtc = hasta.Value.ToUniversalTime();
                query = query.Where(x => x.Pedido.Timestamp <= hastaUtc);
            }

            var result = await query
                .GroupBy(x => new
                {
                    x.Producto.Id,
                    x.Producto.Nombre,
                    x.Producto.Codigo,
                    x.Producto.Categoria
                })
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.Id,
                    Nombre = g.Key.Nombre,
                    Codigo = g.Key.Codigo,
                    Categoria = g.Key.Categoria,
                    CantidadVendida = g.Sum(x => x.Detalle.Cantidad),
                    IngresoTotal = g.Sum(x => x.Detalle.Cantidad * x.Detalle.PrecioUnitario),
                    TotalPedidos = g.Select(x => x.Pedido.Id).Distinct().Count()
                })
                .OrderByDescending(x => x.CantidadVendida)
                .ThenBy(x => x.Nombre)
                .Take(top)
                .ToListAsync();

            return result;
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
                Codigo = dto.Codigo,
                Categoria = dto.Categoria,
                Ubicacion = dto.Ubicacion,
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
            producto.Codigo = dto.Codigo;
            producto.Categoria = dto.Categoria;
            producto.Ubicacion = dto.Ubicacion;
            producto.Descripcion = dto.Descripcion;
            producto.PrecioVenta = dto.PrecioVenta;
            producto.CostoUnitario = dto.CostoUnitario;
            // Handle stock changes through InventoryService so reabastecimientos/orders are created
            var diferenciaStock = dto.StockActual - producto.StockActual;
            if (diferenciaStock > 0)
            {
                var result = await _inventoryService.IncreaseStockAsync(producto.Id, diferenciaStock, dto.Timestamp);
                if (!result.Success) return BadRequest(result.Error);
            }
            else if (diferenciaStock < 0)
            {
                var result = await _inventoryService.DecreaseStockAsync(producto.Id, (int)Math.Abs(diferenciaStock), dto.Timestamp);
                if (!result.Success) return BadRequest(result.Error);
            }

            // Update minimo and other fields (stock actual ya fue actualizado por InventoryService)
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
            var stockBajo = producto.StockActual <= producto.StockMinimo;
            var nivel = stockBajo ? "bajo" : "ok";

            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Codigo = producto.Codigo,
                Categoria = producto.Categoria,
                Ubicacion = producto.Ubicacion,
                Descripcion = producto.Descripcion,
                PrecioVenta = producto.PrecioVenta,
                CostoUnitario = producto.CostoUnitario,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                Estado = producto.Estado,
                CreadoPorId = producto.CreadoPorId,
                Timestamp = producto.Timestamp,
                StockBajo = stockBajo,
                NivelInventario = nivel
            };
        }

        private static IQueryable<Producto> ApplyProductosFilters(
            IQueryable<Producto> query,
            string? search,
            string? categoria,
            string? ubicacion,
            bool? soloBajoStock)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(p => p.Nombre.ToLower().Contains(term) || p.Codigo.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                var cat = categoria.ToLower();
                query = query.Where(p => p.Categoria.ToLower().Contains(cat));
            }

            if (!string.IsNullOrWhiteSpace(ubicacion))
            {
                var ubi = ubicacion.ToLower();
                query = query.Where(p => p.Ubicacion.ToLower().Contains(ubi));
            }

            if (soloBajoStock == true)
            {
                query = query.Where(p => p.StockActual <= p.StockMinimo);
            }

            return query;
        }
    }
}
