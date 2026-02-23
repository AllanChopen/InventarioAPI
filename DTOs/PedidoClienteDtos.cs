using System;

namespace InventarioAPI.DTOs
{
    public class PedidoClienteDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class PedidoClienteCreateDto
    {
        public int ClienteId { get; set; }
        public int Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class PedidoClienteUpdateDto
    {
        public int ClienteId { get; set; }
        public int Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
