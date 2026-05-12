using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Domain.Entities;

namespace Sicoin.Contabilidad.Application.Services;

public class PeriodoService : IPeriodoService
{
    private readonly IContabilidadDbContext _context;

    public PeriodoService(IContabilidadDbContext context)
    {
        _context = context;
    }

    public async Task<List<GestionDto>> ObtenerGestionesAsync(Guid empresaId)
    {
        var gestiones = await _context.Gestiones
            .Include(g => g.Periodos)
            .Where(g => g.EmpresaId == empresaId)
            .OrderByDescending(g => g.Anio)
            .ToListAsync();

        var dtos = new List<GestionDto>();

        foreach (var g in gestiones)
        {
            var pDtos = new List<PeriodoDto>();
            foreach (var p in g.Periodos.OrderBy(x => x.Mes))
            {
                // Calcular Comprobantes
                var comprobantes = await _context.Comprobantes
                    .Where(c => c.EmpresaId == empresaId && c.Gestion == g.Anio && c.PeriodoMes == p.Mes && c.EstadoId == "AC")
                    .ToListAsync();

                pDtos.Add(new PeriodoDto
                {
                    PeriodoId = p.PeriodoId,
                    Mes = p.Mes,
                    EstadoPeriodo = p.EstadoPeriodo,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin,
                    FechaCierreReal = p.FechaCierreReal,
                    UsuarioCierreId = p.UsuarioCierreId,
                    UsuarioCierreNombre = p.UsuarioCierreId.HasValue ? "jperez" : null, // Mocked UI Name
                    ComprobantesCount = comprobantes.Count,
                    BorradoresCount = comprobantes.Count(c => c.EstadoComprobanteId == 1), // 1 = Borrador
                    IvaLiquidado = true, // Mocked dependencies
                    ItLiquidado = true   // Mocked dependencies
                });
            }

            dtos.Add(new GestionDto
            {
                GestionId = g.GestionId,
                EmpresaId = g.EmpresaId,
                Anio = g.Anio,
                EstadoGestion = g.EstadoGestion,
                MonedaBase = g.MonedaBase ?? "BOB",
                Periodos = pDtos
            });
        }

        return dtos;
    }

    public async Task<GestionDto> GestionarNuevaGestionAsync(Guid empresaId, int anio, string monedaBase = "BOB")
    {
        var existe = await _context.Gestiones.AnyAsync(g => g.EmpresaId == empresaId && g.Anio == anio);
        if (existe) throw new Exception("La gestión contable ya se encuentra registrada.");

        var gestion = new Gestion
        {
            EmpresaId = empresaId,
            Anio = anio,
            MonedaBase = monedaBase,
            EstadoGestion = "ABIERTA",
            UsuarioRegistroId = 1 // Hardcode UI
        };

        for (int mes = 1; mes <= 12; mes++)
        {
            var daysInMonth = DateTime.DaysInMonth(anio, mes);
            gestion.Periodos.Add(new Periodo
            {
                Mes = mes,
                EstadoPeriodo = mes < DateTime.UtcNow.Month && anio <= DateTime.UtcNow.Year ? "CERRADO" : (mes == DateTime.UtcNow.Month && anio == DateTime.UtcNow.Year ? "ABIERTO" : "FUTURO"),
                FechaInicio = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc),
                FechaFin = new DateTime(anio, mes, daysInMonth, 23, 59, 59, DateTimeKind.Utc),
                UsuarioRegistroId = 1
            });
        }

        _context.Gestiones.Add(gestion);
        await _context.SaveChangesAsync();

        return (await ObtenerGestionesAsync(empresaId)).FirstOrDefault(x => x.Anio == anio);
    }

    public async Task<PeriodoDto> CerrarPeriodoAsync(int periodoId, long usuarioId)
    {
        var periodo = await _context.Periodos.Include(p => p.Gestion).FirstOrDefaultAsync(x => x.PeriodoId == periodoId);
        if (periodo == null) throw new Exception("Periodo no encontrado.");

        if (periodo.EstadoPeriodo == "CERRADO") throw new Exception("El periodo ya está cerrado.");

        var borradoresPendientes = await _context.Comprobantes
            .CountAsync(c => c.EmpresaId == periodo.Gestion.EmpresaId && c.Gestion == periodo.Gestion.Anio && c.PeriodoMes == periodo.Mes && c.EstadoComprobanteId == 1 && c.EstadoId == "AC");

        if (borradoresPendientes > 0)
        {
            throw new Exception($"No se puede cerrar el periodo. Existen {borradoresPendientes} comprobantes en estado Borrador que deben ser confirmados o eliminados.");
        }

        // Simulación: Validar liquidación de IVA (para cumplir el requerimiento de la UI)
        // En un flujo real llamaríamos al modulo de impuestos.

        periodo.EstadoPeriodo = "CERRADO";
        periodo.FechaCierreReal = DateTime.UtcNow;
        periodo.UsuarioCierreId = usuarioId;
        
        await _context.SaveChangesAsync();

        return new PeriodoDto
        {
            PeriodoId = periodo.PeriodoId,
            Mes = periodo.Mes,
            EstadoPeriodo = periodo.EstadoPeriodo
        };
    }

    public async Task<PeriodoDto> ReabrirPeriodoAsync(int periodoId, long usuarioId)
    {
        var periodo = await _context.Periodos.FirstOrDefaultAsync(x => x.PeriodoId == periodoId);
        if (periodo == null) throw new Exception("Periodo no encontrado.");

        periodo.EstadoPeriodo = "ABIERTO";
        periodo.FechaCierreReal = null;
        periodo.UsuarioCierreId = null;

        await _context.SaveChangesAsync();

        return new PeriodoDto
        {
            PeriodoId = periodo.PeriodoId,
            Mes = periodo.Mes,
            EstadoPeriodo = periodo.EstadoPeriodo
        };
    }
}
