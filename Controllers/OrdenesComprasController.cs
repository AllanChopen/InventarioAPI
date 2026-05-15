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
    public class OrdenesComprasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly InventoryService _inventoryService;

        public OrdenesComprasController(AppDbContext context, InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdenCompraDto>>> GetOrdenesCompras([FromQuery] bool? sugeridas)
        {
            var query = _context.OrdenesCompras
                .Include(o => o.Proveedor)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsQueryable();

            if (sugeridas == true)
            {
                query = query.Where(o => o.Estado == "Sugerida");
            }

            var ordenes = await query.ToListAsync();
            return ordenes.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenCompraDto>> GetOrdenCompra(int id)
        {
            var orden = await _context.OrdenesCompras
                .Include(o => o.Proveedor)
                .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
            {
                return NotFound();
            }

            var dto = ToDto(orden);
            dto.Detalles = orden.Detalles.Select(d => new DTOs.DetalleOrdenCompraDto
            {
                Id = d.Id,
                OrdenId = d.OrdenId,
                ProveedorId = orden.ProveedorId,
                ProveedorNombre = orden.Proveedor?.Nombre ?? string.Empty,
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                CostoUnitario = d.CostoUnitario,
                Timestamp = d.Timestamp,
                CodigoProducto = d.Producto?.Codigo ?? string.Empty,
                NombreProducto = d.Producto?.Nombre ?? string.Empty
            }).ToList();

            return dto;
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

        [HttpPost("{id}/aprobar")]
        public async Task<IActionResult> Aprobar(int id)
        {
            var orden = await _context.OrdenesCompras
                .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
            {
                return NotFound();
            }

            if (orden.Estado == "Aprobada")
            {
                return BadRequest("La orden ya fue aprobada.");
            }

            if (orden.Estado == "Cancelada")
            {
                return BadRequest("No se puede aprobar una orden cancelada.");
            }

            if (orden.Detalles.Count == 0)
            {
                return BadRequest("La orden debe tener al menos un detalle para aprobarse.");
            }

            var timestamp = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var detalle in orden.Detalles)
            {
                var result = await _inventoryService.IncreaseStockAsync(detalle.ProductoId, detalle.Cantidad, timestamp);
                if (!result.Success)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(result.Error);
                }
            }

            orden.Estado = "Aprobada";
            orden.Timestamp = timestamp;

            var reabastecimientos = await _context.Reabastecimientos
                .Where(r => r.OrdenCompraId == orden.Id)
                .ToListAsync();

            foreach (var reabastecimiento in reabastecimientos)
            {
                reabastecimiento.Estado = "Aprobada";
                reabastecimiento.Timestamp = timestamp;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarOrden(int id)
        {
            var orden = await _context.OrdenesCompras.FindAsync(id);
            if (orden == null)
            {
                return NotFound();
            }

            // Unlink from any reabastecimiento and mark reabastecimiento pending
            var reab = await _context.Reabastecimientos.FirstOrDefaultAsync(r => r.OrdenCompraId == id);
            if (reab != null)
            {
                reab.OrdenCompraId = null;
                reab.Estado = "Pendiente";
            }

            orden.Estado = "Cancelada";
            orden.Timestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static OrdenCompraDto ToDto(OrdenCompra orden)
        {
            return new OrdenCompraDto
            {
                Id = orden.Id,
                ProveedorId = orden.ProveedorId,
                ProveedorNombre = orden.Proveedor?.Nombre ?? string.Empty,
                Fecha = orden.Fecha,
                Estado = orden.Estado,
                Timestamp = orden.Timestamp,
                Detalles = orden.Detalles.Select(d => new DetalleOrdenCompraDto
                {
                    Id = d.Id,
                    OrdenId = d.OrdenId,
                    ProveedorId = orden.ProveedorId,
                    ProveedorNombre = orden.Proveedor?.Nombre ?? string.Empty,
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    CostoUnitario = d.CostoUnitario,
                    Timestamp = d.Timestamp,
                    CodigoProducto = d.Producto?.Codigo ?? string.Empty,
                    NombreProducto = d.Producto?.Nombre ?? string.Empty
                }).ToList()
            };
        }
    }
}
