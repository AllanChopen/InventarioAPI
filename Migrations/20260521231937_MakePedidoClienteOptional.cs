using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakePedidoClienteOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pedidos_clientes_clientes_cliente_id",
                table: "pedidos_clientes");

            migrationBuilder.AlterColumn<int>(
                name: "cliente_id",
                table: "pedidos_clientes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_pedidos_clientes_clientes_cliente_id",
                table: "pedidos_clientes",
                column: "cliente_id",
                principalTable: "clientes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pedidos_clientes_clientes_cliente_id",
                table: "pedidos_clientes");

            migrationBuilder.AlterColumn<int>(
                name: "cliente_id",
                table: "pedidos_clientes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_pedidos_clientes_clientes_cliente_id",
                table: "pedidos_clientes",
                column: "cliente_id",
                principalTable: "clientes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
