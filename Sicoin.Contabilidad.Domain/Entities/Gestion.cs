using Sicoin.Common.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Sicoin.Contabilidad.Domain.Entities;

public class Gestion : AuditEntity
{
    public int GestionId { get; set; }
    public Guid EmpresaId { get; set; } // Opcional si es multi-empresa
    public int Anio { get; set; }
    
    // EstadoGestion: "ABIERTA", "CERRADA"
    public string EstadoGestion { get; set; } = "ABIERTA";
    
    // Moneda base: "BOB", "USD", "EUR"
    public string MonedaBase { get; set; } = "BOB";

    public ICollection<Periodo> Periodos { get; set; } = new List<Periodo>();
}
