using System;

namespace Sicoin.Contabilidad.Application.DTOs;

public class ParametroContableDto
{
    public long ParametroId { get; set; }
    public string Clave { get; set; } = null!;
    public string Modulo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string Obligatoriedad { get; set; } = null!;
    public string? CodigoCuenta { get; set; }
    public string? NombreCuenta { get; set; }
    public string TipoCuentaEsperado { get; set; } = null!;
}

public class GrupoParametrosDto
{
    public string Modulo { get; set; } = null!;
    public string IconoRemix { get; set; } = null!;
    public System.Collections.Generic.List<ParametroContableDto> Parametros { get; set; } = new();
}
