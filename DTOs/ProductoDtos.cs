using System;

namespace InventarioAPI.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public decimal CostoUnitario { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool Estado { get; set; }
        public int CreadoPorId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool StockBajo { get; set; }
        public string NivelInventario { get; set; } = string.Empty;
    }

    public class ProductoCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public decimal CostoUnitario { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool Estado { get; set; }
        public int CreadoPorId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ProductoUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public decimal CostoUnitario { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool Estado { get; set; }
        public int CreadoPorId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ProductosResumenDto
    {
        public int TotalProductos { get; set; }
        public int UnidadesTotales { get; set; }
        public int ProductosBajoStock { get; set; }
        public decimal ValorInventarioCosto { get; set; }
        public decimal ValorInventarioVenta { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class ProductoMasVendidoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal IngresoTotal { get; set; }
        public int TotalPedidos { get; set; }
    }
}
