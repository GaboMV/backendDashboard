using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sicoin.Common.Domain.Entities;

namespace Sicoin.Contabilidad.Domain.Entities;

[Table("con_comprobantes")]
public class Comprobante : AuditEntity
{
    [Key]
    [Column("comprobante_id")]
    public long ComprobanteId { get; set; }

    [Column("empresa_id")]
    public Guid EmpresaId { get; set; }

    [Column("sucursal_id")]
    public int? SucursalId { get; set; }

    [Column("tipo_comprobante_id")]
    public int TipoComprobanteId { get; set; }
    
    [Column("tipo_comprobante_descripcion")]
    public string TipoComprobanteDescripcion { get; set; } = string.Empty;

    [Column("nro_comprobante")]
    public string NroComprobante { get; set; } = string.Empty;

    [Column("fecha_contable")]
    public DateTime FechaContable { get; set; }

    [Column("gestion")]
    public int Gestion { get; set; }

    [Column("periodo_mes")]
    public int PeriodoMes { get; set; }

    [Column("concepto")]
    public string Concepto { get; set; } = string.Empty;

    [Column("moneda_id")]
    public int MonedaId { get; set; } // 1: BOB, 2: USD, 3: EUR
    
    [Column("moneda_descripcion")]
    public string MonedaDescripcion { get; set; } = "BOB";

    [Column("tipo_cambio")]
    public decimal TipoCambio { get; set; } = 1.0m;

    [Column("referencia_externa")]
    public string? ReferenciaExterna { get; set; }

    [Column("total_debe")]
    public decimal TotalDebe { get; set; }

    [Column("total_haber")]
    public decimal TotalHaber { get; set; }

    [Column("estado_comprobante_id")]
    public int EstadoComprobanteId { get; set; } // 1: Borrador, 2: Confirmado, 3: Anulado
    
    [Column("estado_comprobante_descripcion")]
    public string EstadoComprobanteDescripcion { get; set; } = string.Empty;

    [Column("es_automatico")]
    public bool EsAutomatico { get; set; }
    
    [Column("motivo_anulacion")]
    public string? MotivoAnulacion { get; set; }

    public virtual ICollection<ComprobanteDetalle> Detalles { get; set; } = new List<ComprobanteDetalle>();
}
