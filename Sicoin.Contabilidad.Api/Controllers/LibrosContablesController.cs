using Microsoft.AspNetCore.Mvc;
using Sicoin.Contabilidad.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Api.Controllers;

[ApiController]
[Route("api/v1/cont/libros")]
public class LibrosContablesController : ControllerBase
{
    private readonly ILibrosContablesService _service;

    public LibrosContablesController(ILibrosContablesService service)
    {
        _service = service;
    }

    /// <summary>
    /// Libro Diario: retorna todos los comprobantes confirmados del rango de períodos.
    /// </summary>
    [HttpGet("diario")]
    public async Task<IActionResult> GetLibroDiario(
        [FromQuery] Guid empresaId,
        [FromQuery] int gestion,
        [FromQuery] int mesDesde = 1,
        [FromQuery] int mesHasta = 12,
        [FromQuery] string? tipos = null,         // "1,2,5" separados por coma
        [FromQuery] bool incluirAnulados = false)
    {
        try
        {
            List<int>? tiposList = null;
            if (!string.IsNullOrWhiteSpace(tipos))
            {
                tiposList = new List<int>();
                foreach (var t in tipos.Split(','))
                    if (int.TryParse(t.Trim(), out int val))
                        tiposList.Add(val);
            }

            var result = await _service.GetLibroDiarioAsync(
                empresaId, gestion, mesDesde, mesHasta, tiposList, incluirAnulados);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Libro Mayor: movimientos por cuenta con saldo acumulado.
    /// </summary>
    [HttpGet("mayor")]
    public async Task<IActionResult> GetLibroMayor(
        [FromQuery] Guid empresaId,
        [FromQuery] int gestion,
        [FromQuery] string cuentaIds,             // "1,5,12" IDs separados por coma
        [FromQuery] int mesDesde = 1,
        [FromQuery] int mesHasta = 12)
    {
        try
        {
            var ids = new List<long>();
            if (!string.IsNullOrWhiteSpace(cuentaIds))
                foreach (var s in cuentaIds.Split(','))
                    if (long.TryParse(s.Trim(), out long id))
                        ids.Add(id);

            if (ids.Count == 0)
                return BadRequest(new { Message = "Debe especificar al menos una cuenta (cuentaIds)." });

            var result = await _service.GetLibroMayorAsync(empresaId, ids, gestion, mesDesde, mesHasta);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Balance de Comprobación: todas las cuentas con saldos iniciales, movimientos y saldos finales.
    /// </summary>
    [HttpGet("balance-comprobacion")]
    public async Task<IActionResult> GetBalanceComprobacion(
        [FromQuery] Guid empresaId,
        [FromQuery] int gestion,
        [FromQuery] int mesDesde = 1,
        [FromQuery] int mesHasta = 12,
        [FromQuery] int nivel = 4)
    {
        try
        {
            var result = await _service.GetBalanceComprobacionAsync(
                empresaId, gestion, mesDesde, mesHasta, nivel);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
