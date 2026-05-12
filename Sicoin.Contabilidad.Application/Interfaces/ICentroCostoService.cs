using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sicoin.Contabilidad.Application.DTOs;

namespace Sicoin.Contabilidad.Application.Interfaces;

public interface ICentroCostoService
{
    Task<List<CentroCostoDto>> ObtenerCentrosAsync(Guid empresaId, string? busqueda, int? tipoId, bool? activo);
    Task<CentroCostoDto> ObtenerCentroPorIdAsync(Guid empresaId, int id);
    Task<int> CrearCentroAsync(Guid empresaId, CentroCostoDto dto);
    Task ActualizarCentroAsync(Guid empresaId, int id, CentroCostoDto dto);
    Task ToggleActivoAsync(Guid empresaId, int id);
    Task EliminarCentroAsync(Guid empresaId, int id);
}
