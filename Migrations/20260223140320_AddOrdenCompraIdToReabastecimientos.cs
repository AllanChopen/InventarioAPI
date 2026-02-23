using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdenCompraIdToReabastecimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "orden_compra_id",
                table: "reabastecimientos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reabastecimientos_orden_compra_id",
                table: "reabastecimientos",
                column: "orden_compra_id");

            migrationBuilder.AddForeignKey(
                name: "FK_reabastecimientos_ordenes_compras_orden_compra_id",
                table: "reabastecimientos",
                column: "orden_compra_id",
                principalTable: "ordenes_compras",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reabastecimientos_ordenes_compras_orden_compra_id",
                table: "reabastecimientos");

            migrationBuilder.DropIndex(
                name: "IX_reabastecimientos_orden_compra_id",
                table: "reabastecimientos");

            migrationBuilder.DropColumn(
                name: "orden_compra_id",
                table: "reabastecimientos");
        }
    }
}
