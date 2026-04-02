using System;

namespace Sicoin.Common.Domain.External;

/// <summary>
/// DTO for Empresa coming from the Central API.
/// </summary>
public class EmpresaDto
{
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    // ...other common fields...
}

/// <summary>
/// DTO for Sucursal coming from the Central/Operations API.
/// </summary>
public class SucursalDto
{
    public int SucursalId { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Proyecto coming from the Operations/Projects API.
/// </summary>
public class ProyectoDto
{
    public int ProyectoId { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
