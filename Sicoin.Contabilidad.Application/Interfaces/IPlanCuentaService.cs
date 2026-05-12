using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Domain.Entities;

namespace Sicoin.Contabilidad.Application.Interfaces;

public interface IPlanCuentaService
{
    Task<PlanCuenta> CreateAsync(PlanCuenta account);
    Task<PlanCuentaDetailDto> GetByIdAsync(long planId, Guid empresaId);
    Task<PlanCuenta> UpdateAsync(PlanCuenta account);
    Task<ImportacionResultadoDto> ImportarCuentasAnaliticasAsync(System.IO.Stream excelStream, Guid empresaId, bool soloValidar, bool sobreescribir);
    Task<List<PlanCuentaDto>> GetTreeAsync(Guid empresaId, string? filter = null);
    Task DeleteAsync(long planId, Guid empresaId);
    Task<List<PlanCuentaDto>> SearchCuentasMovimientoAsync(Guid empresaId, string query);
    Task<List<PlanCuentaDto>> GetCuentasConCentroCostoAsync(Guid empresaId);
}
