using System;

namespace InventarioAPI.DTOs
{
    public class ReabastecimientoDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadSugerida { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int? OrdenCompraId { get; set; }
    }

    public class ReabastecimientoCreateDto
    {
        public int ProductoId { get; set; }
        public int CantidadSugerida { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ReabastecimientoUpdateDto
    {
        public int CantidadSugerida { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
