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
            var query = _context.Productos.AsQueryable();

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

            var productos = await query.ToListAsync();
            return productos.Select(ToDto).ToList();
        }

        [HttpGet("resumen")]
        public async Task<ActionResult<ResumenInventarioDto>> GetResumenInventario(
            [FromQuery] string? search,
            [FromQuery] string? categoria,
            [FromQuery] string? ubicacion,
            [FromQuery] bool? soloBajoStock)
        {
            var query = _context.Productos.AsQueryable();

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

            var resumen = await query
                .GroupBy(_ => 1)
                .Select(g => new ResumenInventarioDto
                {
                    TotalProductos = g.Count(),
                    UnidadesTotales = g.Sum(p => p.StockActual),
                    ProductosBajoStock = g.Count(p => p.StockActual <= p.StockMinimo),
                    ValorInventarioCosto = g.Sum(p => p.StockActual * p.CostoUnitario),
                    ValorInventarioVenta = g.Sum(p => p.StockActual * p.PrecioVenta),
                    GeneratedAt = DateTime.UtcNow.ToString("O")
                })
                .FirstOrDefaultAsync();

            return resumen ?? new ResumenInventarioDto
            {
                TotalProductos = 0,
                UnidadesTotales = 0,
                ProductosBajoStock = 0,
                ValorInventarioCosto = 0,
                ValorInventarioVenta = 0,
                GeneratedAt = DateTime.UtcNow.ToString("O")
            };
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

        [HttpGet("mas-vendidos")]
        public async Task<ActionResult<IEnumerable<ProductoMasVendidoDto>>> GetMasVendidos(
            [FromQuery] int? top,
            [FromQuery] string? desde,
            [FromQuery] string? hasta)
        {
            DateTime? desdeDt = null;
            DateTime? hastaDt = null;
            if (!string.IsNullOrWhiteSpace(desde) && DateTime.TryParse(desde, out var d)) desdeDt = d;
            if (!string.IsNullOrWhiteSpace(hasta) && DateTime.TryParse(hasta, out var h)) hastaDt = h;

            var detalles = _context.DetallesPedidos.AsQueryable();
            if (desdeDt.HasValue) detalles = detalles.Where(x => x.Timestamp >= desdeDt.Value);
            if (hastaDt.HasValue) detalles = detalles.Where(x => x.Timestamp <= hastaDt.Value);

            var query = from det in detalles
                        join p in _context.Productos on det.ProductoId equals p.Id
                        group new { det, p } by new { det.ProductoId, p.Nombre, p.Codigo, p.Categoria } into g
                        select new
                        {
                            ProductoId = g.Key.ProductoId,
                            Nombre = g.Key.Nombre,
                            Codigo = g.Key.Codigo,
                            Categoria = g.Key.Categoria,
                            CantidadVendida = g.Sum(x => x.det.Cantidad),
                            IngresoTotal = g.Sum(x => x.det.Cantidad * x.det.PrecioUnitario),
                            TotalPedidos = g.Select(x => x.det.PedidoId).Distinct().Count()
                        };

            var ordered = query.OrderByDescending(x => x.CantidadVendida).Take(top ?? 10);
            var result = await ordered.ToListAsync();

            var mapped = result.Select(p => new ProductoMasVendidoDto
            {
                ProductoId = p.ProductoId,
                Nombre = p.Nombre ?? string.Empty,
                Codigo = p.Codigo ?? string.Empty,
                Categoria = p.Categoria ?? string.Empty,
                CantidadVendida = p.CantidadVendida,
                IngresoTotal = p.IngresoTotal,
                TotalPedidos = p.TotalPedidos
            }).ToList();

            return mapped;
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
    }
}
