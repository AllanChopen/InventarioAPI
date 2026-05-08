using System;

namespace InventarioAPI.DTOs
{
    public class DetallePedidoDto
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public DateTime Timestamp { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class DetallePedidoCreateDto
    {
        // When creating a detail nested in a pedido, the frontend should send only ProductoId and Cantidad.
        // PedidoId is optional for standalone detalle endpoints; server assigns PrecioUnitario and Timestamp.
        public int? PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class DetallePedidoUpdateDto
    {
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
