using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PawVetAgenda.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CITA",
                columns: table => new
                {
                    id_cita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fecha_hora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duracion_minutos = table.Column<int>(type: "int", nullable: false),
                    motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    id_mascota = table.Column<int>(type: "int", nullable: false),
                    id_veterinario = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CITA", x => x.id_cita);
                    table.CheckConstraint("CK_Cita_Duracion", "[duracion_minutos] BETWEEN 15 AND 120");
                    table.CheckConstraint("CK_Cita_Estado", "[estado] IN ('Agendada', 'Confirmada', 'Atendida', 'Cancelada', 'NoAsistio')");
                });

            migrationBuilder.CreateTable(
                name: "VACUNA",
                columns: table => new
                {
                    id_vacuna = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    id_especie = table.Column<int>(type: "int", nullable: false),
                    intervalo_dias = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VACUNA", x => x.id_vacuna);
                });

            migrationBuilder.CreateTable(
                name: "ATENCION",
                columns: table => new
                {
                    id_atencion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cita = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    diagnostico = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    tratamiento = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    peso = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    temperatura = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    id_mascota = table.Column<int>(type: "int", nullable: false),
                    id_veterinario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATENCION", x => x.id_atencion);
                    table.ForeignKey(
                        name: "FK_Atencion_Cita",
                        column: x => x.id_cita,
                        principalTable: "CITA",
                        principalColumn: "id_cita",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VACUNACION",
                columns: table => new
                {
                    id_vacunacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fecha_aplicacion = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_proxima = table.Column<DateOnly>(type: "date", nullable: true),
                    lote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    id_mascota = table.Column<int>(type: "int", nullable: false),
                    id_veterinario = table.Column<int>(type: "int", nullable: false),
                    id_vacuna = table.Column<int>(type: "int", nullable: false),
                    id_atencion = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VACUNACION", x => x.id_vacunacion);
                    table.ForeignKey(
                        name: "FK_Vacunacion_Atencion",
                        column: x => x.id_atencion,
                        principalTable: "ATENCION",
                        principalColumn: "id_atencion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vacunacion_Vacuna",
                        column: x => x.id_vacuna,
                        principalTable: "VACUNA",
                        principalColumn: "id_vacuna",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "VACUNA",
                columns: new[] { "id_vacuna", "id_especie", "intervalo_dias", "nombre" },
                values: new object[,]
                {
                    { 1, 1, 365, "Antirrábica canina" },
                    { 2, 1, 365, "Óctuple canina" },
                    { 3, 1, 365, "Tos de las perreras" },
                    { 4, 2, 365, "Antirrábica felina" },
                    { 5, 2, 365, "Triple felina" },
                    { 6, 2, 365, "Leucemia felina" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atencion_Mascota",
                table: "ATENCION",
                column: "id_mascota");

            migrationBuilder.CreateIndex(
                name: "UQ_Atencion_Cita",
                table: "ATENCION",
                column: "id_cita",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cita_Veterinario_Fecha",
                table: "CITA",
                columns: new[] { "id_veterinario", "fecha_hora" });

            migrationBuilder.CreateIndex(
                name: "IX_VACUNACION_id_atencion",
                table: "VACUNACION",
                column: "id_atencion");

            migrationBuilder.CreateIndex(
                name: "IX_VACUNACION_id_vacuna",
                table: "VACUNACION",
                column: "id_vacuna");

            migrationBuilder.CreateIndex(
                name: "IX_Vacunacion_Mascota",
                table: "VACUNACION",
                column: "id_mascota");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VACUNACION");

            migrationBuilder.DropTable(
                name: "ATENCION");

            migrationBuilder.DropTable(
                name: "VACUNA");

            migrationBuilder.DropTable(
                name: "CITA");
        }
    }
}
