using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioAPI.Models
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("precio_venta")]
        public decimal PrecioVenta { get; set; }

        [Column("costo_unitario")]
        public decimal CostoUnitario { get; set; }

        [Column("stock_actual")]
        public int StockActual { get; set; }

        [Column("stock_minimo")]
        public int StockMinimo { get; set; }

        [Column("estado")]
        public bool Estado { get; set; }

        [Column("creado_por")]
        public int CreadoPorId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [ForeignKey(nameof(CreadoPorId))]
        public Usuario? CreadoPor { get; set; }

        public ICollection<DetalleOrdenCompra> DetallesOrdenesCompra { get; set; } = new List<DetalleOrdenCompra>();

        public ICollection<DetallePedido> DetallesPedidos { get; set; } = new List<DetallePedido>();

        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }
}
