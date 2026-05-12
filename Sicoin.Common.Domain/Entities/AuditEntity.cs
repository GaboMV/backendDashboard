using System;

namespace Sicoin.Common.Domain.Entities;

public abstract class AuditEntity
{
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }
    public long UsuarioRegistroId { get; set; }
    public long? UsuarioModificacionId { get; set; }
    public string EstadoId { get; set; } = "AC";
}
