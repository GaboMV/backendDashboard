using System.Collections.Generic;

namespace Sicoin.Contabilidad.Application.DTOs;

public class PlanCuentaDto
{
    public long PlanId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public bool AceptaMovimiento { get; set; }
    
    public int TipoCuentaId { get; set; }
    public string TipoCuentaDescripcion { get; set; } = string.Empty;
    
    public int SaldoNormalId { get; set; }
    public string SaldoNormalDescripcion { get; set; } = string.Empty;
    
    public long? PlanPadreId { get; set; }
    public bool AceptaMoneda { get; set; }
    public bool RequiereCosto { get; set; }
    public bool RequiereProyecto { get; set; }
    public string? CodigoSinNandina { get; set; }
    public string? Observacion { get; set; }
    public string EstadoId { get; set; } = "AC";
    
    public List<PlanCuentaDto> Children { get; set; } = new();
}
