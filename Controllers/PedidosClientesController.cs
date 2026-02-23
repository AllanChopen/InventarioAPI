using InventarioAPI.Data;
using InventarioAPI.DTOs;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidosClientesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoClienteDto>>> GetPedidosClientes()
        {
            var pedidos = await _context.PedidosClientes.ToListAsync();
            return pedidos.Select(ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoClienteDto>> GetPedidoCliente(int id)
        {
            var pedido = await _context.PedidosClientes.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            return ToDto(pedido);
        }

        [HttpPost]
        public async Task<ActionResult<PedidoClienteDto>> PostPedidoCliente([FromBody] PedidoClienteCreateDto dto)
        {
            var pedido = new PedidoCliente
            {
                ClienteId = dto.ClienteId,
                Fecha = dto.Fecha,
                Estado = dto.Estado,
                Timestamp = dto.Timestamp
            };

            _context.PedidosClientes.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPedidoCliente), new { id = pedido.Id }, ToDto(pedido));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedidoCliente(int id, [FromBody] PedidoClienteUpdateDto dto)
        {
            var pedido = await _context.PedidosClientes.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            pedido.ClienteId = dto.ClienteId;
            pedido.Fecha = dto.Fecha;
            pedido.Estado = dto.Estado;
            pedido.Timestamp = dto.Timestamp;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedidoCliente(int id)
        {
            var pedido = await _context.PedidosClientes.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            _context.PedidosClientes.Remove(pedido);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static PedidoClienteDto ToDto(PedidoCliente pedido)
        {
            return new PedidoClienteDto
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                Fecha = pedido.Fecha,
                Estado = pedido.Estado,
                Timestamp = pedido.Timestamp
            };
        }
    }
}
