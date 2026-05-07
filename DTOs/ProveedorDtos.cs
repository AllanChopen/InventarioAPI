using System;

namespace InventarioAPI.DTOs
{
    public class ProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public List<ProveedorProductoDto> Productos { get; set; } = new();
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ProveedorCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ProveedorUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ProveedorProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int BodegaId { get; set; }
        public string BodegaNombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }
}
