using System;

namespace InventarioAPI.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int BodegaId { get; set; }
        public string BodegaNombre { get; set; } = string.Empty;
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
        public int CantidadVendida { get; set; }
        public int TotalPedidos { get; set; }
        public decimal IngresoTotal { get; set; }
        public decimal IndiceRotacion { get; set; }
        public string NivelRotacion { get; set; } = string.Empty;
        public double DiasPromedioInventario { get; set; }
    }

    public class ProductoCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public int BodegaId { get; set; }
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
        public int CategoriaId { get; set; }
        public int BodegaId { get; set; }
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

    public class ProductoRotacionDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("productoId")]
        public int ProductoId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("bodega")]
        public string Bodega { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("cantidadVendida")]
        public int CantidadVendida { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("totalPedidos")]
        public int TotalPedidos { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ingresoTotal")]
        public decimal IngresoTotal { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("indiceRotacion")]
        public decimal IndiceRotacion { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("nivelRotacion")]
        public string NivelRotacion { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("diasPromedioInventario")]
        public double DiasPromedioInventario { get; set; }
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
