using InventarioAPI.Data;
using InventarioAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BodegasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BodegasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BodegaDto>>> GetBodegas()
        {
            var bodegas = await _context.Bodegas
                .OrderBy(b => b.Id)
                .Select(b => new BodegaDto
                {
                    Id = b.Id,
                    Nombre = b.Nombre
                })
                .ToListAsync();

            return bodegas;
        }
    }
}