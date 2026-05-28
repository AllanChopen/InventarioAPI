using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioAPI.Models
{
    [Table("pedidos_clientes")]
    public class PedidoCliente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("fecha")]
        public int Fecha { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = string.Empty;

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        // Cliente removed: pedidos ahora solo contienen detalles con producto y cantidad

        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
