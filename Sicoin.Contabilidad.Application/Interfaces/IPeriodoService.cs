using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sicoin.Contabilidad.Application.DTOs;

namespace Sicoin.Contabilidad.Application.Interfaces;

public interface IPeriodoService
{
    Task<List<GestionDto>> ObtenerGestionesAsync(Guid empresaId);
    Task<GestionDto> GestionarNuevaGestionAsync(Guid empresaId, int anio, string monedaBase = "BOB");
    Task<PeriodoDto> CerrarPeriodoAsync(int periodoId, long usuarioId);
    Task<PeriodoDto> ReabrirPeriodoAsync(int periodoId, long usuarioId);
}
