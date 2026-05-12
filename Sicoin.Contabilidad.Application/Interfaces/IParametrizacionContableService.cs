using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sicoin.Contabilidad.Application.DTOs;

namespace Sicoin.Contabilidad.Application.Interfaces;

public interface IParametrizacionContableService
{
    Task<List<GrupoParametrosDto>> ObtenerParametrosAgrupadosAsync(Guid empresaId);
    Task ActualizarParametroAsync(Guid empresaId, string clave, string codigoCuenta);
    Task ActualizarParametrosMasivoAsync(Guid empresaId, List<ParametroContableDto> parametros);
    Task ConfigurarValoresInicialesSiVacioAsync(Guid empresaId);
}
