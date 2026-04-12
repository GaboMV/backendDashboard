using Microsoft.AspNetCore.Mvc;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Api.Controllers;

[ApiController]
[Route("api/v1/cont/plan-cuentas")]
public class PlanCuentaController : ControllerBase
{
    private readonly IPlanCuentaService _planCuentaService;

    public PlanCuentaController(IPlanCuentaService planCuentaService)
    {
        _planCuentaService = planCuentaService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlanCuenta account)
    {
        try
        {
            var createdAccount = await _planCuentaService.CreateAsync(account);
            return CreatedAtAction(nameof(GetTree), new { empresaId = account.EmpresaId }, createdAccount);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("tree/{empresaId:guid}")]
    public async Task<IActionResult> GetTree(Guid empresaId, [FromQuery] string? q = null)
    {
        try
        {
            var tree = await _planCuentaService.GetTreeAsync(empresaId, q);
            return Ok(tree);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpGet("{planId}")]
    public async Task<IActionResult> GetById(long planId, [FromQuery] Guid empresaId)
    {
        try
        {
            var detail = await _planCuentaService.GetByIdAsync(planId, empresaId);
            if (detail == null) return NotFound();
            return Ok(detail);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{planId}")]
    public async Task<IActionResult> Update(long planId, [FromBody] PlanCuenta account)
    {
        try
        {
            if (planId != account.PlanId)
                return BadRequest(new { Message = "ID mismatch." });

            var updatedAccount = await _planCuentaService.UpdateAsync(account);
            return Ok(updatedAccount);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
