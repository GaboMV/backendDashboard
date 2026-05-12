using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sicoin.Common.Domain.Entities;

namespace Sicoin.Contabilidad.Domain.Entities;

[Table("con_planes_cuentas")]
public class PlanCuenta : AuditEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("plan_id")]
    public long PlanId { get; set; }

    [Column("plan_padre_id")]
    public long? PlanPadreId { get; set; }

    [Column("empresa_id")]
    public Guid EmpresaId { get; set; }


    [Column("tipo_cuenta_id")]
    public int TipoCuentaId { get; set; }

    [Column("tipo_cuenta_descripcion")]
    public string TipoCuentaDescripcion { get; set; } = string.Empty;

    [Column("saldo_normal_id")]
    public int SaldoNormalId { get; set; }

    [Column("saldo_normal_descripcion")]
    public string SaldoNormalDescripcion { get; set; } = string.Empty;

    [Column("codigo")]
    public string Codigo { get; set; } = string.Empty;

    [Column("n1")]
    public int N1 { get; set; }

    [Column("n2")]
    public int? N2 { get; set; }

    [Column("n3")]
    public int? N3 { get; set; }

    [Column("n4")]
    public string? N4 { get; set; }

    [Column("n5")]
    public string? N5 { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("nivel")]
    public int Nivel { get; set; }

    [Column("acepta_movimiento")]
    public bool AceptaMovimiento { get; set; }

    [Column("acepta_moneda")]
    public bool AceptaMoneda { get; set; }

    [Column("requiere_costo")]
    public bool RequiereCosto { get; set; }

    [Column("requiere_proyecto")]
    public bool RequiereProyecto { get; set; }

    [Column("codigo_sin_nandina")]
    public string? CodigoSinNandina { get; set; }

    [Column("observacion")]
    public string? Observacion { get; set; }

    [Column("cuenta_ajuste_id")]
    public long? CuentaAjusteId { get; set; }

    [ForeignKey("CuentaAjusteId")]
    public virtual PlanCuenta? CuentaAjuste { get; set; }

    public virtual ICollection<PlanCuenta> PlanesCuentasHijos { get; set; } = new List<PlanCuenta>();
}
