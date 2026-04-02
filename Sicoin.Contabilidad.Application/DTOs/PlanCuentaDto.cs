using System.Collections.Generic;

namespace Sicoin.Contabilidad.Application.DTOs;

public class PlanCuentaDto
{
    public long PlanId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public bool AceptaMovimiento { get; set; }
    public string TipoCuentaDescripcion { get; set; } = string.Empty;
    public string SaldoNormalDescripcion { get; set; } = string.Empty;
    public List<PlanCuentaDto> Children { get; set; } = new();
}
