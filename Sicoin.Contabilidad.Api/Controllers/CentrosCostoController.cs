using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;

namespace Sicoin.Contabilidad.Api.Controllers;

[ApiController]
[Route("api/v1/cont/[controller]")]
public class CentrosCostoController : ControllerBase
{
    private readonly ICentroCostoService _service;

    public CentrosCostoController(ICentroCostoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid empresaId, 
        [FromQuery] string? busqueda, 
        [FromQuery] int? tipoId, 
        [FromQuery] bool? activo)
    {
        if (empresaId == Guid.Empty) return BadRequest("EmpresaId es requerido");

        var result = await _service.ObtenerCentrosAsync(empresaId, busqueda, tipoId, activo);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener([FromQuery] Guid empresaId, int id)
    {
        try
        {
            var result = await _service.ObtenerCentroPorIdAsync(empresaId, id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromQuery] Guid empresaId, [FromBody] CentroCostoDto dto)
    {
        try
        {
            var id = await _service.CrearCentroAsync(empresaId, dto);
            return CreatedAtAction(nameof(Obtener), new { empresaId, id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar([FromQuery] Guid empresaId, int id, [FromBody] CentroCostoDto dto)
    {
        try
        {
            await _service.ActualizarCentroAsync(empresaId, id, dto);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/toggle-activo")]
    public async Task<IActionResult> ToggleActivo([FromQuery] Guid empresaId, int id)
    {
        try
        {
            await _service.ToggleActivoAsync(empresaId, id);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar([FromQuery] Guid empresaId, int id)
    {
        try
        {
            await _service.EliminarCentroAsync(empresaId, id);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
