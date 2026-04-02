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
    Task<List<PlanCuentaDto>> GetTreeAsync(Guid empresaId);
}
