using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sicoin.Contabilidad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAccountingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "con_comprobantes",
                columns: table => new
                {
                    comprobante_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sucursal_id = table.Column<int>(type: "integer", nullable: true),
                    tipo_comprobante_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_comprobante_descripcion = table.Column<string>(type: "text", nullable: false),
                    nro_comprobante = table.Column<string>(type: "text", nullable: false),
                    fecha_contable = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gestion = table.Column<int>(type: "integer", nullable: false),
                    periodo_mes = table.Column<int>(type: "integer", nullable: false),
                    concepto = table.Column<string>(type: "text", nullable: false),
                    moneda_id = table.Column<int>(type: "integer", nullable: false),
                    moneda_descripcion = table.Column<string>(type: "text", nullable: false),
                    tipo_cambio = table.Column<decimal>(type: "numeric", nullable: false),
                    referencia_externa = table.Column<string>(type: "text", nullable: true),
                    total_debe = table.Column<decimal>(type: "numeric", nullable: false),
                    total_haber = table.Column<decimal>(type: "numeric", nullable: false),
                    estado_comprobante_id = table.Column<int>(type: "integer", nullable: false),
                    estado_comprobante_descripcion = table.Column<string>(type: "text", nullable: false),
                    es_automatico = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_anulacion = table.Column<string>(type: "text", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioRegistroId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioModificacionId = table.Column<long>(type: "bigint", nullable: true),
                    EstadoId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_con_comprobantes", x => x.comprobante_id);
                });

            migrationBuilder.CreateTable(
                name: "con_planes_cuentas",
                columns: table => new
                {
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    plan_padre_id = table.Column<long>(type: "bigint", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_cuenta_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_cuenta_descripcion = table.Column<string>(type: "text", nullable: false),
                    saldo_normal_id = table.Column<int>(type: "integer", nullable: false),
                    saldo_normal_descripcion = table.Column<string>(type: "text", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    n1 = table.Column<int>(type: "integer", nullable: false),
                    n2 = table.Column<int>(type: "integer", nullable: true),
                    n3 = table.Column<int>(type: "integer", nullable: true),
                    n4 = table.Column<string>(type: "text", nullable: true),
                    n5 = table.Column<string>(type: "text", nullable: true),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    acepta_movimiento = table.Column<bool>(type: "boolean", nullable: false),
                    acepta_moneda = table.Column<bool>(type: "boolean", nullable: false),
                    requiere_costo = table.Column<bool>(type: "boolean", nullable: false),
                    requiere_proyecto = table.Column<bool>(type: "boolean", nullable: false),
                    codigo_sin_nandina = table.Column<string>(type: "text", nullable: true),
                    observacion = table.Column<string>(type: "text", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioRegistroId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioModificacionId = table.Column<long>(type: "bigint", nullable: true),
                    EstadoId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_con_planes_cuentas", x => x.plan_id);
                    table.ForeignKey(
                        name: "FK_con_planes_cuentas_con_planes_cuentas_plan_padre_id",
                        column: x => x.plan_padre_id,
                        principalTable: "con_planes_cuentas",
                        principalColumn: "plan_id");
                });

            migrationBuilder.CreateTable(
                name: "con_comprobante_detalles",
                columns: table => new
                {
                    comprobante_detalle_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    comprobante_id = table.Column<long>(type: "bigint", nullable: false),
                    nro_linea = table.Column<int>(type: "integer", nullable: false),
                    cuenta_id = table.Column<long>(type: "bigint", nullable: false),
                    centro_costo_id = table.Column<int>(type: "integer", nullable: true),
                    proyecto_id = table.Column<int>(type: "integer", nullable: true),
                    glosa = table.Column<string>(type: "text", nullable: true),
                    debe = table.Column<decimal>(type: "numeric", nullable: false),
                    haber = table.Column<decimal>(type: "numeric", nullable: false),
                    auxiliar_referencia = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_con_comprobante_detalles", x => x.comprobante_detalle_id);
                    table.ForeignKey(
                        name: "FK_con_comprobante_detalles_con_comprobantes_comprobante_id",
                        column: x => x.comprobante_id,
                        principalTable: "con_comprobantes",
                        principalColumn: "comprobante_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_con_comprobante_detalles_con_planes_cuentas_cuenta_id",
                        column: x => x.cuenta_id,
                        principalTable: "con_planes_cuentas",
                        principalColumn: "plan_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_con_comprobante_detalles_comprobante_id",
                table: "con_comprobante_detalles",
                column: "comprobante_id");

            migrationBuilder.CreateIndex(
                name: "IX_con_comprobante_detalles_cuenta_id",
                table: "con_comprobante_detalles",
                column: "cuenta_id");

            migrationBuilder.CreateIndex(
                name: "IX_con_planes_cuentas_plan_padre_id",
                table: "con_planes_cuentas",
                column: "plan_padre_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "con_comprobante_detalles");

            migrationBuilder.DropTable(
                name: "con_comprobantes");

            migrationBuilder.DropTable(
                name: "con_planes_cuentas");
        }
    }
}
