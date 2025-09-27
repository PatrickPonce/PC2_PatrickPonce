using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PC2_PatrickPonce.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInmuebles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Inmuebles",
                columns: new[] { "Id", "Activo", "Banos", "Ciudad", "Codigo", "Direccion", "Dormitorios", "Imagen", "MetrosCuadrados", "Precio", "Tipo", "Titulo" },
                values: new object[,]
                {
                    { 1, true, 1, "Lima", "A001", "Av. Principal 123", 2, "departamento1.jpg", 80.0, 150000.0, 0, "Apartamento céntrico" },
                    { 2, true, 2, "Arequipa", "C001", "Calle Los Olivos 456", 3, "casa1.jpg", 120.0, 250000.0, 1, "Casa con jardín" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Inmuebles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Inmuebles",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
