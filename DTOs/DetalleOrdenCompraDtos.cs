using System;

namespace InventarioAPI.DTOs
{
    public class DetalleOrdenCompraDto
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime Timestamp { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
    }

    public class DetalleOrdenCompraCreateDto
    {
        public int OrdenId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DetalleOrdenCompraUpdateDto
    {
        public int OrdenId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
