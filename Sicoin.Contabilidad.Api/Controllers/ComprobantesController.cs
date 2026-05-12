using Microsoft.AspNetCore.Mvc;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Api.Controllers;

[ApiController]
[Route("api/v1/cont/comprobantes")]
public class ComprobantesController : ControllerBase
{
    private readonly IComprobanteService _service;

    public ComprobantesController(IComprobanteService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] ComprobanteRequestDto request)
    {
        try
        {
            var result = await _service.CrearComprobanteAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/anular")]
    public async Task<IActionResult> Anular(long id, [FromBody] string motivo)
    {
        var result = await _service.AnularComprobanteAsync(id, motivo);
        if (!result) return NotFound();
        return Ok(new { Message = "Comprobante anulado correctamente." });
    }
}
