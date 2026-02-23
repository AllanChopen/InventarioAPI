using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProveedorSugeridoToReabastecimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "proveedor_sugerido_id",
                table: "reabastecimientos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reabastecimientos_proveedor_sugerido_id",
                table: "reabastecimientos",
                column: "proveedor_sugerido_id");

            migrationBuilder.AddForeignKey(
                name: "FK_reabastecimientos_proveedores_proveedor_sugerido_id",
                table: "reabastecimientos",
                column: "proveedor_sugerido_id",
                principalTable: "proveedores",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reabastecimientos_proveedores_proveedor_sugerido_id",
                table: "reabastecimientos");

            migrationBuilder.DropIndex(
                name: "IX_reabastecimientos_proveedor_sugerido_id",
                table: "reabastecimientos");

            migrationBuilder.DropColumn(
                name: "proveedor_sugerido_id",
                table: "reabastecimientos");
        }
    }
}
