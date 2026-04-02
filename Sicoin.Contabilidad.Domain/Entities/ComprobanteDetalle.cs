using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sicoin.Contabilidad.Domain.Entities;

[Table("con_comprobante_detalles")]
public class ComprobanteDetalle
{
    [Key]
    [Column("comprobante_detalle_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ComprobanteDetalleId { get; set; }

    [Column("comprobante_id")]
    public long ComprobanteId { get; set; }

    [Column("nro_linea")]
    public int NroLinea { get; set; }

    [Column("cuenta_id")]
    public long CuentaId { get; set; }

    [Column("centro_costo_id")]
    public int? CentroCostoId { get; set; }

    [Column("proyecto_id")]
    public int? ProyectoId { get; set; }

    [Column("glosa")]
    public string? Glosa { get; set; }

    [Column("debe")]
    public decimal Debe { get; set; }

    [Column("haber")]
    public decimal Haber { get; set; }
    
    // External generic reference like Proveedor / Cliente if needed
    [Column("auxiliar_referencia")]
    public string? AuxiliarReferencia { get; set; }

    [ForeignKey("ComprobanteId")]
    public virtual Comprobante Comprobante { get; set; } = null!;

    [ForeignKey("CuentaId")]
    public virtual PlanCuenta PlanCuenta { get; set; } = null!;
}
