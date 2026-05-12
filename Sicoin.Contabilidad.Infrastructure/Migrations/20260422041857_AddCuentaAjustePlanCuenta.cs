using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sicoin.Contabilidad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCuentaAjustePlanCuenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "cuenta_ajuste_id",
                table: "con_planes_cuentas",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_con_planes_cuentas_cuenta_ajuste_id",
                table: "con_planes_cuentas",
                column: "cuenta_ajuste_id");

            migrationBuilder.AddForeignKey(
                name: "FK_con_planes_cuentas_con_planes_cuentas_cuenta_ajuste_id",
                table: "con_planes_cuentas",
                column: "cuenta_ajuste_id",
                principalTable: "con_planes_cuentas",
                principalColumn: "plan_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_con_planes_cuentas_con_planes_cuentas_cuenta_ajuste_id",
                table: "con_planes_cuentas");

            migrationBuilder.DropIndex(
                name: "IX_con_planes_cuentas_cuenta_ajuste_id",
                table: "con_planes_cuentas");

            migrationBuilder.DropColumn(
                name: "cuenta_ajuste_id",
                table: "con_planes_cuentas");
        }
    }
}
