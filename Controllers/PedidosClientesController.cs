using InventarioAPI.Data;
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
        public async Task<ActionResult<IEnumerable<PedidoCliente>>> GetPedidosClientes()
        {
            return await _context.PedidosClientes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoCliente>> GetPedidoCliente(int id)
        {
            var pedido = await _context.PedidosClientes.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            return pedido;
        }

        [HttpPost]
        public async Task<ActionResult<PedidoCliente>> PostPedidoCliente(PedidoCliente pedidoCliente)
        {
            _context.PedidosClientes.Add(pedidoCliente);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPedidoCliente), new { id = pedidoCliente.Id }, pedidoCliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedidoCliente(int id, PedidoCliente pedidoCliente)
        {
            if (id != pedidoCliente.Id)
            {
                return BadRequest();
            }

            var exists = await _context.PedidosClientes.AnyAsync(p => p.Id == id);
            if (!exists)
            {
                return NotFound();
            }

            _context.Entry(pedidoCliente).State = EntityState.Modified;
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
    }
}
