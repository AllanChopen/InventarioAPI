using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioAPI.Models
{
    [Table("reabastecimientos")]
    public class Reabastecimiento
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Column("cantidad_sugerida")]
        public int CantidadSugerida { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobada, Cancelada

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [ForeignKey(nameof(ProductoId))]
        public Producto? Producto { get; set; }
        
        [Column("proveedor_sugerido_id")]
        public int? ProveedorSugeridoId { get; set; }

        [ForeignKey(nameof(ProveedorSugeridoId))]
        public Proveedor? ProveedorSugerido { get; set; }

        [Column("orden_compra_id")]
        public int? OrdenCompraId { get; set; }

        [ForeignKey(nameof(OrdenCompraId))]
        public OrdenCompra? OrdenCompra { get; set; }
    }
}
