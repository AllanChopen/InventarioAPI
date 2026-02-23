using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "categoria",
                table: "productos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "productos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ubicacion",
                table: "productos",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Convert cantidad to integer using explicit cast
            migrationBuilder.Sql("ALTER TABLE movimientos ALTER COLUMN cantidad TYPE integer USING cantidad::integer;");

            migrationBuilder.CreateIndex(
                name: "IX_productos_codigo",
                table: "productos",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_productos_codigo",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "categoria",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "ubicacion",
                table: "productos");

            migrationBuilder.Sql("ALTER TABLE movimientos ALTER COLUMN cantidad TYPE text;");
        }
    }
}
