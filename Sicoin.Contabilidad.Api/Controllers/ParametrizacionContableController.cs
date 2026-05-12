using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;

namespace Sicoin.Contabilidad.Api.Controllers;

[ApiController]
[Route("api/v1/cont/[controller]")]
public class ParametrizacionContableController : ControllerBase
{
    private readonly IParametrizacionContableService _service;

    public ParametrizacionContableController(IParametrizacionContableService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerParametros([FromQuery] Guid empresaId)
    {
        if (empresaId == Guid.Empty) return BadRequest("EmpresaId es requerido");

        // Configuración por primera vez si no existen
        await _service.ConfigurarValoresInicialesSiVacioAsync(empresaId);

        var agrupados = await _service.ObtenerParametrosAgrupadosAsync(empresaId);
        return Ok(agrupados);
    }

    [HttpPut("{clave}")]
    public async Task<IActionResult> ActualizarParametro([FromRoute] string clave, [FromQuery] Guid empresaId, [FromBody] UpdateParametroRequest request)
    {
        if (empresaId == Guid.Empty) return BadRequest("EmpresaId es requerido");

        try
        {
            await _service.ActualizarParametroAsync(empresaId, clave, request.CodigoCuenta ?? "");
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("masivo")]
    public async Task<IActionResult> ActualizarMasivo([FromQuery] Guid empresaId, [FromBody] List<ParametroContableDto> parametros)
    {
        if (empresaId == Guid.Empty) return BadRequest("EmpresaId es requerido");

        try
        {
            await _service.ActualizarParametrosMasivoAsync(empresaId, parametros);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

public class UpdateParametroRequest
{
    public string? CodigoCuenta { get; set; }
}
