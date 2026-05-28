using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClienteFromPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pedidos_clientes_clientes_cliente_id",
                table: "pedidos_clientes");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_clientes_cliente_id",
                table: "pedidos_clientes");

            migrationBuilder.DropColumn(
                name: "cliente_id",
                table: "pedidos_clientes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cliente_id",
                table: "pedidos_clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_clientes_cliente_id",
                table: "pedidos_clientes",
                column: "cliente_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pedidos_clientes_clientes_cliente_id",
                table: "pedidos_clientes",
                column: "cliente_id",
                principalTable: "clientes",
                principalColumn: "id");
        }
    }
}
