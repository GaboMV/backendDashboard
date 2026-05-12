using Sicoin.Contabilidad.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Application.Interfaces;

public interface ILibrosContablesService
{
    Task<LibroDiarioResponseDto> GetLibroDiarioAsync(
        Guid empresaId,
        int gestion,
        int mesDesde,
        int mesHasta,
        List<int>? tiposComprobante = null,
        bool incluirAnulados = false);

    Task<LibroMayorResponseDto> GetLibroMayorAsync(
        Guid empresaId,
        List<long> cuentaIds,
        int gestion,
        int mesDesde,
        int mesHasta);

    Task<BalanceComprobacionResponseDto> GetBalanceComprobacionAsync(
        Guid empresaId,
        int gestion,
        int mesDesde,
        int mesHasta,
        int nivelMaximo = 4);
}
