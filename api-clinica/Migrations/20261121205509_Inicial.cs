using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PawVetClinica.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ESPECIE",
                columns: table => new
                {
                    id_especie = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESPECIE", x => x.id_especie);
                });

            migrationBuilder.CreateTable(
                name: "ROL",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROL", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "TUTOR",
                columns: table => new
                {
                    id_tutor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rut = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TUTOR", x => x.id_tutor);
                });

            migrationBuilder.CreateTable(
                name: "RAZA",
                columns: table => new
                {
                    id_raza = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    id_especie = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RAZA", x => x.id_raza);
                    table.ForeignKey(
                        name: "FK_Raza_Especie",
                        column: x => x.id_especie,
                        principalTable: "ESPECIE",
                        principalColumn: "id_especie",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    id_user = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    id_rol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.id_user);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol",
                        column: x => x.id_rol,
                        principalTable: "ROL",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MASCOTA",
                columns: table => new
                {
                    id_mascota = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    sexo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    peso = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    microchip = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    id_tutor = table.Column<int>(type: "int", nullable: false),
                    id_especie = table.Column<int>(type: "int", nullable: false),
                    id_raza = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MASCOTA", x => x.id_mascota);
                    table.CheckConstraint("CK_Mascota_Sexo", "[sexo] IN ('Macho', 'Hembra')");
                    table.ForeignKey(
                        name: "FK_Mascota_Especie",
                        column: x => x.id_especie,
                        principalTable: "ESPECIE",
                        principalColumn: "id_especie",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mascota_Raza",
                        column: x => x.id_raza,
                        principalTable: "RAZA",
                        principalColumn: "id_raza",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mascota_Tutor",
                        column: x => x.id_tutor,
                        principalTable: "TUTOR",
                        principalColumn: "id_tutor");
                });

            migrationBuilder.CreateTable(
                name: "VETERINARIO",
                columns: table => new
                {
                    id_veterinario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rut = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    especialidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    id_user = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VETERINARIO", x => x.id_veterinario);
                    table.ForeignKey(
                        name: "FK_Veterinario_Usuario",
                        column: x => x.id_user,
                        principalTable: "USUARIO",
                        principalColumn: "id_user",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "ESPECIE",
                columns: new[] { "id_especie", "nombre" },
                values: new object[,]
                {
                    { 1, "Perro" },
                    { 2, "Gato" }
                });

            migrationBuilder.InsertData(
                table: "ROL",
                columns: new[] { "id_rol", "nombre" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Recepcionista" },
                    { 3, "Veterinario" }
                });

            migrationBuilder.InsertData(
                table: "RAZA",
                columns: new[] { "id_raza", "id_especie", "nombre" },
                values: new object[,]
                {
                    { 1, 1, "Labrador Retriever" },
                    { 2, 1, "Pastor Alemán" },
                    { 3, 1, "Poodle" },
                    { 4, 1, "Quiltro" },
                    { 5, 2, "Siamés" },
                    { 6, 2, "Persa" },
                    { 7, 2, "Doméstico de pelo corto" }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Especie_Nombre",
                table: "ESPECIE",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MASCOTA_id_especie",
                table: "MASCOTA",
                column: "id_especie");

            migrationBuilder.CreateIndex(
                name: "IX_MASCOTA_id_raza",
                table: "MASCOTA",
                column: "id_raza");

            migrationBuilder.CreateIndex(
                name: "IX_MASCOTA_id_tutor",
                table: "MASCOTA",
                column: "id_tutor");

            migrationBuilder.CreateIndex(
                name: "UQ_Mascota_Microchip",
                table: "MASCOTA",
                column: "microchip",
                unique: true,
                filter: "[microchip] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RAZA_id_especie",
                table: "RAZA",
                column: "id_especie");

            migrationBuilder.CreateIndex(
                name: "UQ_Rol_Nombre",
                table: "ROL",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Tutor_Rut",
                table: "TUTOR",
                column: "rut",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_id_rol",
                table: "USUARIO",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "UQ_Usuario_Email",
                table: "USUARIO",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Veterinario_Rut",
                table: "VETERINARIO",
                column: "rut",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Veterinario_Usuario",
                table: "VETERINARIO",
                column: "id_user",
                unique: true,
                filter: "[id_user] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MASCOTA");

            migrationBuilder.DropTable(
                name: "VETERINARIO");

            migrationBuilder.DropTable(
                name: "RAZA");

            migrationBuilder.DropTable(
                name: "TUTOR");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "ESPECIE");

            migrationBuilder.DropTable(
                name: "ROL");
        }
    }
}
