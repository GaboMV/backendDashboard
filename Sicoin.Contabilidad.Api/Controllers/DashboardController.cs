using Microsoft.AspNetCore.Mvc;
using Sicoin.Contabilidad.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Api.Controllers;

[ApiController]
[Route("api/v1/cont/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("{empresaId:guid}")]
    public async Task<IActionResult> GetSummary(Guid empresaId, [FromQuery] int? sucursalId, [FromQuery] int gestion, [FromQuery] int periodo)
    {
        try
        {
            // Simple validation
            if (gestion == 0 || periodo == 0)
            {
                return BadRequest(new { Message = "Gestion and Periodo are required." });
            }

            var result = await _dashboardService.GetDashboardSummaryAsync(empresaId, sucursalId, gestion, periodo);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
