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
    }
}
