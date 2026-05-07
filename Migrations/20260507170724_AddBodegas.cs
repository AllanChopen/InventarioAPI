using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventarioAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBodegas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bodega_id",
                table: "productos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "bodegas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bodegas", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "bodegas",
                columns: new[] { "id", "nombre" },
                values: new object[,]
                {
                    { 1, "Bodega Central" },
                    { 2, "Bodega Sur" },
                    { 3, "Bodega Norte" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_productos_bodega_id",
                table: "productos",
                column: "bodega_id");

            migrationBuilder.AddForeignKey(
                name: "FK_productos_bodegas_bodega_id",
                table: "productos",
                column: "bodega_id",
                principalTable: "bodegas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_productos_bodegas_bodega_id",
                table: "productos");

            migrationBuilder.DropTable(
                name: "bodegas");

            migrationBuilder.DropIndex(
                name: "IX_productos_bodega_id",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "bodega_id",
                table: "productos");
        }
    }
}
