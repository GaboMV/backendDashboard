using Sicoin.Common.Domain.Entities;
using System;

namespace Sicoin.Contabilidad.Domain.Entities;

public class Periodo : AuditEntity
{
    public int PeriodoId { get; set; }
    
    public int GestionId { get; set; }
    public Gestion Gestion { get; set; }
    
    public int Mes { get; set; } // 1 a 12
    
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    // ABIERTO, CERRADO, FUTURO
    public string EstadoPeriodo { get; set; } = "FUTURO";
    
    // Fechas de auditoria del cierre
    public DateTime? FechaCierreReal { get; set; }
    public long? UsuarioCierreId { get; set; }
}
