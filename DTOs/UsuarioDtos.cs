using System;

namespace InventarioAPI.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class UsuarioCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class UsuarioUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Contrasena { get; set; }
        public string Rol { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
