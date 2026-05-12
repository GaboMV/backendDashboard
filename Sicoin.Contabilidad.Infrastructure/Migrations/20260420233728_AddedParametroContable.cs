using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sicoin.Contabilidad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedParametroContable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParametrosContables",
                columns: table => new
                {
                    ParametroId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Clave = table.Column<string>(type: "text", nullable: false),
                    Modulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Obligatoriedad = table.Column<string>(type: "text", nullable: false),
                    CodigoCuenta = table.Column<string>(type: "text", nullable: true),
                    NombreCuenta = table.Column<string>(type: "text", nullable: true),
                    TipoCuentaEsperado = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioRegistroId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioModificacionId = table.Column<long>(type: "bigint", nullable: true),
                    EstadoId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosContables", x => x.ParametroId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParametrosContables");
        }
    }
}
