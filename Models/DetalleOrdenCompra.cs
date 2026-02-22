using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioAPI.Models
{
    [Table("detalles_ordenes_compras")]
    public class DetalleOrdenCompra
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("orden_id")]
        public int OrdenId { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("costo_unitario")]
        public decimal CostoUnitario { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [ForeignKey(nameof(OrdenId))]
        public OrdenCompra? Orden { get; set; }

        [ForeignKey(nameof(ProductoId))]
        public Producto? Producto { get; set; }
    }
}
