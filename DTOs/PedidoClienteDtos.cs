using System;
using System.Collections.Generic;

namespace InventarioAPI.DTOs
{
    public class PedidoClienteDto
    {
        public int Id { get; set; }
        public int Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        // Cliente removed: pedidos no exponen cliente
        public decimal Total { get; set; }
        public List<DetallePedidoDto> Detalles { get; set; } = new List<DetallePedidoDto>();
    }

    public class PedidoClienteCreateDto
    {
        public List<DetallePedidoCreateDto> Detalles { get; set; } = new List<DetallePedidoCreateDto>();
    }

    public class PedidoClienteUpdateDto
    {
        public int Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
