using System;

namespace InventarioAPI.DTOs
{
    public class MovimientoDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Cantidad { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class MovimientoCreateDto
    {
        public int ProductoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Cantidad { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class MovimientoUpdateDto
    {
        public int ProductoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Cantidad { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
