using System;

namespace InventarioAPI.DTOs
{
    public class VentasPorProductoDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal IngresoTotal { get; set; }
    }

    public class VentasPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public int CantidadUnidades { get; set; }
        public decimal IngresoTotal { get; set; }
    }

    public class ReporteVentasDto
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public decimal TotalVentas { get; set; }
        public int TotalUnidades { get; set; }
        public int TotalPedidos { get; set; }
        public decimal TicketPromedio { get; set; }
        public List<VentasPorProductoDto> VentasPorProducto { get; set; } = new();
        public List<VentasPorDiaDto> VentasPorDia { get; set; } = new();
    }
}
