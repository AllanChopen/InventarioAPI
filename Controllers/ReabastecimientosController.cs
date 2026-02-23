using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReabastecimientosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<InventarioAPI.Hubs.InventoryHub> _hubContext;

        public ReabastecimientosController(AppDbContext context, Microsoft.AspNetCore.SignalR.IHubContext<InventarioAPI.Hubs.InventoryHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReabastecimientoDto>>> GetAll()
        {
            var items = await _context.Reabastecimientos
                .Include(r => r.Producto)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();

            return items.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReabastecimientoDto>> GetById(int id)
        {
            var item = await _context.Reabastecimientos
                .Include(r => r.Producto)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return ToDto(item);
        }

        [HttpPost]
        public async Task<ActionResult<ReabastecimientoDto>> Create([FromBody] ReabastecimientoCreateDto dto)
        {
            var producto = await _context.Productos.FindAsync(dto.ProductoId);
            if (producto == null)
            {
                return BadRequest("Producto no encontrado.");
            }

            var entity = new Reabastecimiento
            {
                ProductoId = dto.ProductoId,
                CantidadSugerida = dto.CantidadSugerida,
                Estado = "Pendiente",
                Timestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp
            };

            _context.Reabastecimientos.Add(entity);
            await _context.SaveChangesAsync();

            await _context.Entry(entity).Reference(e => e.Producto).LoadAsync();

            // Reload to include OrdenCompraId if set by InventoryService
            await _context.Entry(entity).Reference(e => e.OrdenCompra).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReabastecimientoUpdateDto dto)
        {
            var entity = await _context.Reabastecimientos.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.CantidadSugerida = dto.CantidadSugerida;
            // allow updating suggested proveedor
            if (dto.ProveedorSugeridoId.HasValue)
            {
                entity.ProveedorSugeridoId = dto.ProveedorSugeridoId.Value;
            }
            entity.Estado = dto.Estado;
            entity.Timestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/aprobar")]
        public async Task<IActionResult> Aprobar(int id)
        {
            var entity = await _context.Reabastecimientos.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            // Create OrdenCompra when approving the reabastecimiento
            var proveedorId = entity.ProveedorSugeridoId ?? await _context.Proveedores.Select(p => p.Id).FirstOrDefaultAsync();
            if (proveedorId == 0)
            {
                var proveedorFallback = new Proveedor
                {
                    Nombre = "Proveedor Sugerido",
                    Telefono = string.Empty,
                    Email = string.Empty,
                    Direccion = string.Empty,
                    Estado = true,
                    Timestamp = DateTime.UtcNow
                };

                _context.Proveedores.Add(proveedorFallback);
                await _context.SaveChangesAsync();
                proveedorId = proveedorFallback.Id;
            }

            var orden = new OrdenCompra
            {
                ProveedorId = proveedorId,
                Fecha = DateTime.UtcNow,
                Estado = "Creada",
                Timestamp = DateTime.UtcNow
            };

            _context.OrdenesCompras.Add(orden);
            await _context.SaveChangesAsync();

            var producto = await _context.Productos.FindAsync(entity.ProductoId);
            var detalle = new DetalleOrdenCompra
            {
                OrdenId = orden.Id,
                ProductoId = entity.ProductoId,
                Cantidad = entity.CantidadSugerida,
                CostoUnitario = producto?.CostoUnitario ?? 0m,
                Timestamp = DateTime.UtcNow
            };

            _context.DetallesOrdenesCompras.Add(detalle);

            // Link and update reabastecimiento
            entity.OrdenCompraId = orden.Id;
            entity.Estado = "Aprobada";
            entity.Timestamp = DateTime.UtcNow;

            _context.Reabastecimientos.Update(entity);
            await _context.SaveChangesAsync();

            // Notify via SignalR
            await _hubContext.Clients.All.SendAsync("OrdenCreadaPorAprobacion", new
            {
                OrdenId = orden.Id,
                ReabastecimientoId = entity.Id,
                ProveedorId = orden.ProveedorId
            });

            return NoContent();
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var entity = await _context.Reabastecimientos.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Estado = "Cancelada";
            entity.Timestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static ReabastecimientoDto ToDto(Reabastecimiento r)
        {
            return new ReabastecimientoDto
            {
                Id = r.Id,
                ProductoId = r.ProductoId,
                CodigoProducto = r.Producto?.Codigo ?? string.Empty,
                NombreProducto = r.Producto?.Nombre ?? string.Empty,
                CantidadSugerida = r.CantidadSugerida,
                Estado = r.Estado,
                Timestamp = r.Timestamp,
                OrdenCompraId = r.OrdenCompraId,
                ProveedorSugeridoId = r.ProveedorSugeridoId
            };
        }
    }
}
