using System;
using System.Collections.Generic;

namespace Sicoin.Contabilidad.Application.DTOs;

public class ComprobanteRequestDto
{
    public Guid EmpresaId { get; set; }
    public int? SucursalId { get; set; }
    public int TipoComprobanteId { get; set; } // 1: Ingreso, 2: Egreso, 3: Traspaso, 4: Compra, 5: Venta
    public DateTime Fecha { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public int MonedaId { get; set; } // 1: BOB, 2: USD
    public decimal TipoCambio { get; set; } = 1.0m;
    public string? ReferenciaExterna { get; set; }
    public List<ComprobanteDetalleRequestDto> Detalles { get; set; } = new();
}

public class ComprobanteDetalleRequestDto
{
    public long CuentaId { get; set; }
    public int? CentroCostoId { get; set; }
    public string? Glosa { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string? AuxiliarReferencia { get; set; } // Nombre del Proveedor/Cliente
}

public class ComprobanteSimpleResponseDto
{
    public long ComprobanteId { get; set; }
    public string NroComprobante { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
