using Sicoin.Contabilidad.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponseDto> GetDashboardSummaryAsync(Guid empresaId, int? sucursalId, int gestion, int periodoMes);
}
