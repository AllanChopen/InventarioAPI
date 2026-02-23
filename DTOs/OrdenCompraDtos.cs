using System;

namespace InventarioAPI.DTOs
{
    public class OrdenCompraDto
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class OrdenCompraCreateDto
    {
        public int ProveedorId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class OrdenCompraUpdateDto
    {
        public int ProveedorId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
