using System;

namespace InventarioAPI.DTOs
{
    public class DetallePedidoDto
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DetallePedidoCreateDto
    {
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DetallePedidoUpdateDto
    {
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
