using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioAPI.Services;
using System.Globalization;

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
            [FromQuery] string? bodega,
            [FromQuery] bool? soloBajoStock)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Bodega)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(p => p.Nombre.ToLower().Contains(term) || p.Codigo.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                var cat = categoria.ToLower();
                query = query.Where(p => p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(cat));
            }

            if (!string.IsNullOrWhiteSpace(bodega))
            {
                var nombreBodega = bodega.ToLower();
                query = query.Where(p => p.Bodega != null && p.Bodega.Nombre.ToLower().Contains(nombreBodega));
            }

            if (soloBajoStock == true)
            {
                query = query.Where(p => p.StockActual <= p.StockMinimo);
            }

            var productos = await query.ToListAsync();
            var metricas = await GetRotationMetricsAsync(productos, DateTime.UtcNow);
            return productos.Select(p => ToDto(p, metricas.GetValueOrDefault(p.Id))).ToList();
        }

        [HttpGet("resumen")]
        public async Task<ActionResult<ResumenInventarioDto>> GetResumenInventario(
            [FromQuery] string? search,
            [FromQuery] string? categoria,
            [FromQuery] string? bodega,
            [FromQuery] bool? soloBajoStock)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Bodega)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(p => p.Nombre.ToLower().Contains(term) || p.Codigo.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                var cat = categoria.ToLower();
                query = query.Where(p => p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(cat));
            }

            if (!string.IsNullOrWhiteSpace(bodega))
            {
                var nombreBodega = bodega.ToLower();
                query = query.Where(p => p.Bodega != null && p.Bodega.Nombre.ToLower().Contains(nombreBodega));
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
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Bodega)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            var metricas = await GetRotationMetricsAsync(new[] { producto }, DateTime.UtcNow);
            return ToDto(producto, metricas.GetValueOrDefault(producto.Id));
        }

        [HttpPost]
        public async Task<ActionResult<ProductoDto>> PostProducto([FromBody] ProductoCreateDto dto)
        {
            if (!await _context.Bodegas.AnyAsync(b => b.Id == dto.BodegaId))
            {
                return BadRequest("La bodega seleccionada no existe.");
            }

            if (!await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
            {
                return BadRequest("La categoria seleccionada no existe.");
            }

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Codigo = $"TMP-{Guid.NewGuid():N}",
                CategoriaId = dto.CategoriaId,
                BodegaId = dto.BodegaId,
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

            producto.Codigo = GenerateProductCode(producto.Id);
            await _context.SaveChangesAsync();

            await _context.Entry(producto)
                .Reference(p => p.Bodega)
                .LoadAsync();

            await _context.Entry(producto)
                .Reference(p => p.Categoria)
                .LoadAsync();

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

            if (!await _context.Bodegas.AnyAsync(b => b.Id == dto.BodegaId))
            {
                return BadRequest("La bodega seleccionada no existe.");
            }

            if (!await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
            {
                return BadRequest("La categoria seleccionada no existe.");
            }

            producto.Nombre = dto.Nombre;
            producto.CategoriaId = dto.CategoriaId;
            producto.BodegaId = dto.BodegaId;
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
                        group new { det, p } by new { det.ProductoId, p.Nombre, p.Codigo, Categoria = p.Categoria != null ? p.Categoria.Nombre : string.Empty } into g
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

        [HttpGet("rotacion")]
        public async Task<ActionResult<IEnumerable<ProductoRotacionDto>>> GetRotacionProductos(
            [FromQuery] int? top,
            [FromQuery] string? desde,
            [FromQuery] string? hasta)
        {
            DateTime? desdeDt = null;
            DateTime? hastaDt = null;

            if (!string.IsNullOrWhiteSpace(desde) && DateTime.TryParse(desde, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var d))
            {
                desdeDt = d;
            }

            if (!string.IsNullOrWhiteSpace(hasta) && DateTime.TryParse(hasta, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var h))
            {
                hastaDt = h;
            }

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Bodega)
                .ToListAsync();

            var productoIds = productos.Select(p => p.Id).ToList();

            var detallesQuery = _context.DetallesPedidos
                .Where(d => productoIds.Contains(d.ProductoId));

            if (desdeDt.HasValue)
            {
                detallesQuery = detallesQuery.Where(d => d.Timestamp >= desdeDt.Value);
            }

            if (hastaDt.HasValue)
            {
                detallesQuery = detallesQuery.Where(d => d.Timestamp <= hastaDt.Value);
            }

            var detalles = await detallesQuery.ToListAsync();

            var entradasQuery = _context.Movimientos
                .Where(m => productoIds.Contains(m.ProductoId) && m.Tipo == "Entrada");

            if (hastaDt.HasValue)
            {
                entradasQuery = entradasQuery.Where(m => m.Timestamp <= hastaDt.Value);
            }

            var entradas = await entradasQuery.ToListAsync();
            var fechaReferencia = hastaDt ?? DateTime.UtcNow;

            var result = productos
                .Select(producto => BuildRotacionDto(producto, detalles, entradas, fechaReferencia))
                .OrderByDescending(x => x.IndiceRotacion)
                .ThenByDescending(x => x.CantidadVendida)
                .ThenBy(x => x.Nombre)
                .Take(top ?? int.MaxValue)
                .ToList();

            return result;
        }

        private async Task<Dictionary<int, ProductoRotacionDto>> GetRotationMetricsAsync(
            IReadOnlyCollection<Producto> productos,
            DateTime fechaReferencia)
        {
            var productoIds = productos.Select(p => p.Id).ToList();
            if (productoIds.Count == 0)
            {
                return new Dictionary<int, ProductoRotacionDto>();
            }

            var detalles = await _context.DetallesPedidos
                .Where(d => productoIds.Contains(d.ProductoId))
                .ToListAsync();

            var entradas = await _context.Movimientos
                .Where(m => productoIds.Contains(m.ProductoId) && m.Tipo == "Entrada")
                .ToListAsync();

            return productos.ToDictionary(
                producto => producto.Id,
                producto => BuildRotacionDto(producto, detalles, entradas, fechaReferencia));
        }

        private static ProductoDto ToDto(Producto producto, ProductoRotacionDto? rotacion = null)
        {
            var stockBajo = producto.StockActual <= producto.StockMinimo;
            var nivel = stockBajo ? "bajo" : "ok";

            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Codigo = producto.Codigo,
                CategoriaId = producto.CategoriaId,
                Categoria = producto.Categoria?.Nombre ?? string.Empty,
                BodegaId = producto.BodegaId,
                BodegaNombre = producto.Bodega?.Nombre ?? string.Empty,
                Descripcion = producto.Descripcion,
                PrecioVenta = producto.PrecioVenta,
                CostoUnitario = producto.CostoUnitario,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                Estado = producto.Estado,
                CreadoPorId = producto.CreadoPorId,
                Timestamp = producto.Timestamp,
                StockBajo = stockBajo,
                NivelInventario = nivel,
                CantidadVendida = rotacion?.CantidadVendida ?? 0,
                TotalPedidos = rotacion?.TotalPedidos ?? 0,
                IngresoTotal = rotacion?.IngresoTotal ?? 0,
                IndiceRotacion = rotacion?.IndiceRotacion ?? 0,
                NivelRotacion = rotacion?.NivelRotacion ?? "baja",
                DiasPromedioInventario = rotacion?.DiasPromedioInventario ?? 0
            };
        }

        private static string GenerateProductCode(int productId)
        {
            return productId.ToString("D6");
        }

        private static ProductoRotacionDto BuildRotacionDto(
            Producto producto,
            IReadOnlyCollection<DetallePedido> detalles,
            IReadOnlyCollection<Movimiento> entradas,
            DateTime fechaReferencia)
        {
            var ventasProducto = detalles
                .Where(d => d.ProductoId == producto.Id)
                .OrderBy(d => d.Timestamp)
                .ToList();

            var entradasProducto = entradas
                .Where(m => m.ProductoId == producto.Id)
                .Select(m => m.Timestamp)
                .OrderBy(t => t)
                .ToList();

            var cantidadVendida = ventasProducto.Sum(v => v.Cantidad);
            var totalPedidos = ventasProducto.Select(v => v.PedidoId).Distinct().Count();
            var ingresoTotal = ventasProducto.Sum(v => v.Cantidad * v.PrecioUnitario);

            var inventarioPromedio = ((producto.StockActual + cantidadVendida) + producto.StockActual) / 2m;
            var indiceRotacion = inventarioPromedio <= 0 ? 0 : Math.Round(cantidadVendida / inventarioPromedio, 2);
            var diasPromedio = CalculateAverageDaysInInventory(producto.Timestamp, ventasProducto, entradasProducto, fechaReferencia);

            return new ProductoRotacionDto
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                Codigo = producto.Codigo,
                Categoria = producto.Categoria?.Nombre ?? string.Empty,
                Bodega = producto.Bodega?.Nombre ?? string.Empty,
                CantidadVendida = cantidadVendida,
                TotalPedidos = totalPedidos,
                IngresoTotal = ingresoTotal,
                IndiceRotacion = indiceRotacion,
                NivelRotacion = GetRotationLevel(indiceRotacion),
                DiasPromedioInventario = Math.Round(diasPromedio, 2)
            };
        }

        private static double CalculateAverageDaysInInventory(
            DateTime fechaCreacion,
            IReadOnlyList<DetallePedido> ventasProducto,
            IReadOnlyList<DateTime> entradasProducto,
            DateTime fechaReferencia)
        {
            if (ventasProducto.Count == 0)
            {
                var ultimaEntrada = entradasProducto.LastOrDefault();
                var baseDate = ultimaEntrada == default ? fechaCreacion : ultimaEntrada;
                return Math.Max((fechaReferencia - baseDate).TotalDays, 0);
            }

            var acumuladoDias = 0d;

            foreach (var venta in ventasProducto)
            {
                var fechaBase = fechaCreacion;

                foreach (var entrada in entradasProducto)
                {
                    if (entrada > venta.Timestamp)
                    {
                        break;
                    }

                    fechaBase = entrada;
                }

                acumuladoDias += Math.Max((venta.Timestamp - fechaBase).TotalDays, 0);
            }

            return acumuladoDias / ventasProducto.Count;
        }

        private static string GetRotationLevel(decimal indiceRotacion)
        {
            if (indiceRotacion >= 1m) return "alta";
            if (indiceRotacion >= 0.3m) return "media";
            return "baja";
        }
    }
}
