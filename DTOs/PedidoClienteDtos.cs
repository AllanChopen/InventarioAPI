using System;

namespace InventarioAPI.DTOs
{
    public class PedidoClienteDetalleDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PedidoClienteDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public decimal TotalVenta { get; set; }
        public List<PedidoClienteDetalleDto> Detalles { get; set; } = new();
    }

    public class PedidoClienteDetalleCreateDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    public class PedidoClienteCreateDto
    {
        public int ClienteId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
        public List<PedidoClienteDetalleCreateDto> Detalles { get; set; } = new();
    }

    public class PedidoClienteUpdateDto
    {
        public int ClienteId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
        public List<PedidoClienteDetalleCreateDto> Detalles { get; set; } = new();
    }
}
