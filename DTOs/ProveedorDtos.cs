using System;

namespace InventarioAPI.DTOs
{
    public class ProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ProveedorCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ProveedorUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
