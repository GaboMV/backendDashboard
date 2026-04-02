using Sicoin.Common.Domain.Entities;

namespace Sicoin.Clasificadores.Domain.Entities;

public class TipoCuenta : AuditEntity
{
    public int TipoCuentaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class TipoComprobante : AuditEntity
{
    public int TipoComprobanteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class EstadoPeriodo : AuditEntity
{
    public int EstadoPeriodoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class Moneda : AuditEntity
{
    public string MonedaId { get; set; } = string.Empty; // e.g., "BOB", "USD"
    public string Nombre { get; set; } = string.Empty;
}

public class TipoCentroCosto : AuditEntity
{
    public int TipoCentroCostoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
