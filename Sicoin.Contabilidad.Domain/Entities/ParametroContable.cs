using System;
using Sicoin.Common.Domain.Entities;

namespace Sicoin.Contabilidad.Domain.Entities;

public class ParametroContable : AuditEntity
{
    public long ParametroId { get; set; }
    public Guid EmpresaId { get; set; }
    
    // e.g. IVA_CREDITO_FISCAL_LOCAL, CXP_PROVEEDORES_LOCAL, VENTAS_MERCANCIAS
    public string Clave { get; set; } = null!;
    
    // Compras, Ventas, Inventarios, Tesoreria, ContabilidadGeneral
    public string Modulo { get; set; } = null!;
    
    public string Descripcion { get; set; } = null!;
    
    // Si, No, Condicional
    public string Obligatoriedad { get; set; } = null!;
    
    // 1.1.05.001
    public string? CodigoCuenta { get; set; }
    
    // IVA Crédito Fiscal Local
    public string? NombreCuenta { get; set; }
    
    // Activo, Pasivo, Ingreso, Costo, Gasto, Patrimonio
    public string TipoCuentaEsperado { get; set; } = null!;
}
