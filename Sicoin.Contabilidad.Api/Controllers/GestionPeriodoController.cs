using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sicoin.Contabilidad.Application.Interfaces;

namespace Sicoin.Contabilidad.Api.Controllers;

[ApiController]
[Route("api/v1/cont/gestiones")]
public class GestionPeriodoController : ControllerBase
{
    private readonly IPeriodoService _periodoService;

    public GestionPeriodoController(IPeriodoService periodoService)
    {
        _periodoService = periodoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetGestiones([FromQuery] Guid empresaId)
    {
        try
        {
            var result = await _periodoService.ObtenerGestionesAsync(empresaId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CrearGestion([FromQuery] Guid empresaId, [FromQuery] int anio, [FromQuery] string monedaBase = "BOB")
    {
        try
        {
            var result = await _periodoService.GestionarNuevaGestionAsync(empresaId, anio, monedaBase);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("periodos/{idPeriodo}/cerrar")]
    public async Task<IActionResult> CerrarPeriodo(int idPeriodo)
    {
        try
        {
            long mockUsuarioId = 1; // Reemplazar con ID del token en el futuro
            var result = await _periodoService.CerrarPeriodoAsync(idPeriodo, mockUsuarioId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("periodos/{idPeriodo}/reabrir")]
    public async Task<IActionResult> ReabrirPeriodo(int idPeriodo)
    {
        try
        {
            long mockUsuarioId = 1; // Reemplazar con ID del token en el futuro
            var result = await _periodoService.ReabrirPeriodoAsync(idPeriodo, mockUsuarioId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
