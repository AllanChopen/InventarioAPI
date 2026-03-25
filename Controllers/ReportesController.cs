using InventarioAPI.Data;
using InventarioAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace InventarioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ventas")]
        public async Task<ActionResult<ReporteVentasDto>> GetReporteVentas(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var reporte = await BuildReporteVentas(desde, hasta);
            if (reporte == null)
            {
                return BadRequest("El rango de fechas es inválido: 'desde' debe ser menor o igual a 'hasta'.");
            }

            return reporte;
        }

        [HttpGet("ventas/csv")]
        public async Task<IActionResult> GetReporteVentasCsv(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var reporte = await BuildReporteVentas(desde, hasta);
            if (reporte == null)
            {
                return BadRequest("El rango de fechas es inválido: 'desde' debe ser menor o igual a 'hasta'.");
            }

            var csv = new StringBuilder();
            csv.AppendLine("Seccion,Campo,Valor");
            csv.AppendLine($"Resumen,Desde,{FormatDate(reporte.Desde)}");
            csv.AppendLine($"Resumen,Hasta,{FormatDate(reporte.Hasta)}");
            csv.AppendLine($"Resumen,TotalVentas,{reporte.TotalVentas.ToString(CultureInfo.InvariantCulture)}");
            csv.AppendLine($"Resumen,TotalUnidades,{reporte.TotalUnidades}");
            csv.AppendLine($"Resumen,TotalPedidos,{reporte.TotalPedidos}");
            csv.AppendLine($"Resumen,TicketPromedio,{reporte.TicketPromedio.ToString(CultureInfo.InvariantCulture)}");
            csv.AppendLine();

            csv.AppendLine("VentasPorProducto,ProductoId,NombreProducto,CantidadVendida,IngresoTotal");
            foreach (var item in reporte.VentasPorProducto)
            {
                csv.AppendLine($"VentasPorProducto,{item.ProductoId},\"{EscapeCsv(item.NombreProducto)}\",{item.CantidadVendida},{item.IngresoTotal.ToString(CultureInfo.InvariantCulture)}");
            }
            csv.AppendLine();

            csv.AppendLine("VentasPorDia,Fecha,CantidadUnidades,IngresoTotal");
            foreach (var item in reporte.VentasPorDia)
            {
                csv.AppendLine($"VentasPorDia,{item.Fecha:yyyy-MM-dd},{item.CantidadUnidades},{item.IngresoTotal.ToString(CultureInfo.InvariantCulture)}");
            }

            var fileName = $"reporte-ventas-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        private async Task<ReporteVentasDto?> BuildReporteVentas(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            {
                return null;
            }

            var query = from d in _context.DetallesPedidos
                        join p in _context.PedidosClientes on d.PedidoId equals p.Id
                        join pr in _context.Productos on d.ProductoId equals pr.Id
                        where p.Estado.ToLower() == "entregado"
                        select new
                        {
                            PedidoId = p.Id,
                            FechaPedido = p.Timestamp,
                            ProductoId = d.ProductoId,
                            NombreProducto = pr.Nombre,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario
                        };

            if (desde.HasValue)
            {
                var desdeUtc = desde.Value.ToUniversalTime();
                query = query.Where(x => x.FechaPedido >= desdeUtc);
            }

            if (hasta.HasValue)
            {
                var hastaUtc = hasta.Value.ToUniversalTime();
                query = query.Where(x => x.FechaPedido <= hastaUtc);
            }

            var rows = await query.ToListAsync();

            var totalVentas = rows.Sum(x => x.Cantidad * x.PrecioUnitario);
            var totalUnidades = rows.Sum(x => x.Cantidad);
            var totalPedidos = rows.Select(x => x.PedidoId).Distinct().Count();
            var ticketPromedio = totalPedidos > 0 ? totalVentas / totalPedidos : 0;

            var ventasPorProducto = rows
                .GroupBy(x => new { x.ProductoId, x.NombreProducto })
                .Select(g => new VentasPorProductoDto
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.NombreProducto,
                    CantidadVendida = g.Sum(x => x.Cantidad),
                    IngresoTotal = g.Sum(x => x.Cantidad * x.PrecioUnitario)
                })
                .OrderByDescending(x => x.IngresoTotal)
                .ThenBy(x => x.NombreProducto)
                .ToList();

            var ventasPorDia = rows
                .GroupBy(x => x.FechaPedido.Date)
                .Select(g => new VentasPorDiaDto
                {
                    Fecha = g.Key,
                    CantidadUnidades = g.Sum(x => x.Cantidad),
                    IngresoTotal = g.Sum(x => x.Cantidad * x.PrecioUnitario)
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            return new ReporteVentasDto
            {
                Desde = desde,
                Hasta = hasta,
                TotalVentas = totalVentas,
                TotalUnidades = totalUnidades,
                TotalPedidos = totalPedidos,
                TicketPromedio = ticketPromedio,
                VentasPorProducto = ventasPorProducto,
                VentasPorDia = ventasPorDia
            };
        }

        private static string EscapeCsv(string value)
        {
            return (value ?? string.Empty).Replace("\"", "\"\"");
        }

        private static string FormatDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : string.Empty;
        }
    }
}
