using System;

namespace Sicoin.Contabilidad.Application.DTOs;

public class PlanCuentaDetailDto
{
    public long PlanId { get; set; }
    public long? PlanPadreId { get; set; }
    public string PlanPadreCodigoNombre { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public int TipoCuentaId { get; set; }
    public int SaldoNormalId { get; set; }
    public string SaldoNormalDescripcion { get; set; } = string.Empty;
    public bool AceptaMovimiento { get; set; }
    public bool AceptaMoneda { get; set; }
    public bool RequiereCosto { get; set; }
    public bool RequiereProyecto { get; set; }
    public string? CodigoSinNandina { get; set; }
    public string EstadoId { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public decimal SaldoActual { get; set; }
    public int MovimientoCount { get; set; }
}
