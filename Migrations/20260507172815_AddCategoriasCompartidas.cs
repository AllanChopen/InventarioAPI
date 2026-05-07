using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventarioAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriasCompartidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "categorias",
                columns: new[] { "id", "nombre" },
                values: new object[,]
                {
                    { 1, "Hardware" },
                    { 2, "Periféricos" },
                    { 3, "Audio" },
                    { 4, "Monitores" }
                });

            migrationBuilder.AddColumn<int>(
                name: "categoria_id",
                table: "proveedores",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "categoria_id",
                table: "productos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
                UPDATE productos
                SET categoria_id = CASE
                    WHEN lower(categoria) = 'hardware' THEN 1
                    WHEN lower(categoria) IN ('periféricos', 'perifericos') THEN 2
                    WHEN lower(categoria) = 'audio' THEN 3
                    WHEN lower(categoria) = 'monitores' THEN 4
                    ELSE 1
                END;
            ");

            migrationBuilder.DropColumn(
                name: "categoria",
                table: "productos");

            migrationBuilder.CreateIndex(
                name: "IX_proveedores_categoria_id",
                table: "proveedores",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_productos_categoria_id",
                table: "productos",
                column: "categoria_id");

            migrationBuilder.AddForeignKey(
                name: "FK_productos_categorias_categoria_id",
                table: "productos",
                column: "categoria_id",
                principalTable: "categorias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_proveedores_categorias_categoria_id",
                table: "proveedores",
                column: "categoria_id",
                principalTable: "categorias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_productos_categorias_categoria_id",
                table: "productos");

            migrationBuilder.DropForeignKey(
                name: "FK_proveedores_categorias_categoria_id",
                table: "proveedores");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropIndex(
                name: "IX_proveedores_categoria_id",
                table: "proveedores");

            migrationBuilder.DropIndex(
                name: "IX_productos_categoria_id",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "categoria_id",
                table: "proveedores");

            migrationBuilder.DropColumn(
                name: "categoria_id",
                table: "productos");

            migrationBuilder.AddColumn<string>(
                name: "categoria",
                table: "productos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
