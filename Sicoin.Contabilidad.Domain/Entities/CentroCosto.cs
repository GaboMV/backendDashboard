using System;
using System.Collections.Generic;
using Sicoin.Common.Domain.Entities;

namespace Sicoin.Contabilidad.Domain.Entities;

public class CentroCosto : AuditEntity
{
    public int CentroCostoId { get; set; }
    public Guid EmpresaId { get; set; }
    
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    
    public int TipoId { get; set; }
    
    // Auto-referencia a un posible centro superior (ej: "Ventas" como padre de "Ventas LP")
    public int? CentroPadreId { get; set; }
    public virtual CentroCosto? CentroPadre { get; set; }
    
    // Lista de centros subordinados a este
    public virtual ICollection<CentroCosto> CentrosHijos { get; set; } = new List<CentroCosto>();
    
    public string? Responsable { get; set; }
    
    // A falta de un módulo de presupuestos real, lo guardamos nominalmente.
    public decimal PresupuestoAnual { get; set; }
    
    public bool Activo { get; set; }
}
