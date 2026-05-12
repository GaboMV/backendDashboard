using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sicoin.Contabilidad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCentrosCosto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CentrosCosto",
                columns: table => new
                {
                    CentroCostoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    TipoId = table.Column<int>(type: "integer", nullable: false),
                    CentroPadreId = table.Column<int>(type: "integer", nullable: true),
                    Responsable = table.Column<string>(type: "text", nullable: true),
                    PresupuestoAnual = table.Column<decimal>(type: "numeric", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioRegistroId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioModificacionId = table.Column<long>(type: "bigint", nullable: true),
                    EstadoId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentrosCosto", x => x.CentroCostoId);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_CentrosCosto_CentroPadreId",
                        column: x => x.CentroPadreId,
                        principalTable: "CentrosCosto",
                        principalColumn: "CentroCostoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_CentroPadreId",
                table: "CentrosCosto",
                column: "CentroPadreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CentrosCosto");
        }
    }
}
