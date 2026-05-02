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

    public class ProductoMasVendidoDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("productoId")]
        public int ProductoId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("cantidadVendida")]
        public int CantidadVendida { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ingresoTotal")]
        public decimal IngresoTotal { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("totalPedidos")]
        public int TotalPedidos { get; set; }
    }

    public class ResumenInventarioDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("totalProductos")]
        public int TotalProductos { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("unidadesTotales")]
        public int UnidadesTotales { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("productosBajoStock")]
        public int ProductosBajoStock { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("valorInventarioCosto")]
        public decimal ValorInventarioCosto { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("valorInventarioVenta")]
        public decimal ValorInventarioVenta { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("generatedAt")]
        public string GeneratedAt { get; set; } = string.Empty;
    }
}
