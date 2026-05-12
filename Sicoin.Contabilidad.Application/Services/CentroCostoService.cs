using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Domain.Entities;

namespace Sicoin.Contabilidad.Application.Services;

public class CentroCostoService : ICentroCostoService
{
    private readonly IContabilidadDbContext _context;

    public CentroCostoService(IContabilidadDbContext context)
    {
        _context = context;
    }

    public async Task<List<CentroCostoDto>> ObtenerCentrosAsync(Guid empresaId, string? busqueda, int? tipoId, bool? activo)
    {
        var query = _context.CentrosCosto
            .Include(c => c.CentroPadre)
            .Where(c => c.EmpresaId == empresaId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(busqueda))
        {
            var bLower = busqueda.ToLower();
            query = query.Where(c => c.Codigo.ToLower().Contains(bLower) || 
                                     c.Nombre.ToLower().Contains(bLower));
        }

        if (tipoId.HasValue)
            query = query.Where(c => c.TipoId == tipoId.Value);

        if (activo.HasValue)
            query = query.Where(c => c.Activo == activo.Value);

        var list = await query.ToListAsync();

        return list.Select(c => new CentroCostoDto
        {
            Id = c.CentroCostoId,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
            TipoId = c.TipoId,
            CentroPadreId = c.CentroPadreId,
            CentroPadreCodigo = c.CentroPadre?.Codigo,
            CentroPadreNombre = c.CentroPadre?.Nombre,
            Responsable = c.Responsable,
            PresupuestoAnual = c.PresupuestoAnual, 
            GastoAcumulado = 0m,
            TotalMovimientos = 0, 
            Activo = c.Activo
        }).ToList();
    }

    public async Task<CentroCostoDto> ObtenerCentroPorIdAsync(Guid empresaId, int id)
    {
        var c = await _context.CentrosCosto
            .Include(x => x.CentroPadre)
            .FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.CentroCostoId == id);
            
        if (c == null) throw new ArgumentException("Centro de costo no encontrado.");

        return new CentroCostoDto
        {
            Id = c.CentroCostoId,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
            TipoId = c.TipoId,
            CentroPadreId = c.CentroPadreId,
            CentroPadreCodigo = c.CentroPadre?.Codigo,
            CentroPadreNombre = c.CentroPadre?.Nombre,
            Responsable = c.Responsable,
            PresupuestoAnual = c.PresupuestoAnual,
            GastoAcumulado = 0m,
            TotalMovimientos = 0,
            Activo = c.Activo
        };
    }

    public async Task<int> CrearCentroAsync(Guid empresaId, CentroCostoDto dto)
    {
        var existeCodigo = await _context.CentrosCosto.AnyAsync(c => c.EmpresaId == empresaId && c.Codigo == dto.Codigo);
        if (existeCodigo)
            throw new ArgumentException($"Validación V-001: El código '{dto.Codigo}' ya existe.");

        var nuevo = new CentroCosto
        {
            EmpresaId = empresaId,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            TipoId = dto.TipoId,
            CentroPadreId = dto.CentroPadreId,
            Responsable = dto.Responsable,
            Activo = dto.Activo,
            PresupuestoAnual = dto.PresupuestoAnual,
            FechaRegistro = DateTime.UtcNow,
            UsuarioRegistroId = 1, // DUMMY USER
            EstadoId = "AC"
        };

        _context.CentrosCosto.Add(nuevo);
        await _context.SaveChangesAsync();

        return nuevo.CentroCostoId;
    }

    public async Task ActualizarCentroAsync(Guid empresaId, int id, CentroCostoDto dto)
    {
        var existeCodigo = await _context.CentrosCosto.AnyAsync(c => c.EmpresaId == empresaId && c.Codigo == dto.Codigo && c.CentroCostoId != id);
        if (existeCodigo)
            throw new ArgumentException($"Validación V-001: El código '{dto.Codigo}' ya está usado en otro centro.");

        var centro = await _context.CentrosCosto.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.CentroCostoId == id);
        if (centro == null) throw new ArgumentException("Centro no encontrado.");

        if (centro.CentroPadreId != dto.CentroPadreId && dto.CentroPadreId.HasValue)
        {
            // Validacion recursividad ciruclar super basica: que no se ponga de padre a su propio hijo
            if (dto.CentroPadreId.Value == centro.CentroCostoId)
                throw new ArgumentException("No se puede asignar el centro a si mismo como padre.");
        }

        centro.Codigo = dto.Codigo;
        centro.Nombre = dto.Nombre;
        centro.TipoId = dto.TipoId;
        centro.CentroPadreId = dto.CentroPadreId;
        centro.Responsable = dto.Responsable;
        centro.Activo = dto.Activo;
        centro.PresupuestoAnual = dto.PresupuestoAnual;
        centro.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ToggleActivoAsync(Guid empresaId, int id)
    {
        var centro = await _context.CentrosCosto
            .Include(c => c.CentrosHijos)
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.CentroCostoId == id);
            
        if (centro == null) throw new ArgumentException("Centro no encontrado.");

        if (centro.Activo)
        {
            // Validacion V-004 mejorada: no desactivar si tiene hijos activos (la logica usual)
            if (centro.CentrosHijos.Any(h => h.Activo))
                throw new ArgumentException("Validación V-004: No se puede desactivar un centro padre si tiene sub-centros activos.");
        }
        else
        {
            // Si lo vamos a activar y tiene padre, el padre debe estar activo
            if (centro.CentroPadreId.HasValue)
            {
                var padre = await _context.CentrosCosto.FindAsync(centro.CentroPadreId.Value);
                if (padre != null && !padre.Activo)
                    throw new ArgumentException("No se puede activar el centro porque su centro jerárquico superior está desactivado.");
            }
        }

        centro.Activo = !centro.Activo;
        await _context.SaveChangesAsync();
    }

    public async Task EliminarCentroAsync(Guid empresaId, int id)
    {
        var centro = await _context.CentrosCosto
            .Include(c => c.CentrosHijos)
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.CentroCostoId == id);
            
        if (centro == null) throw new ArgumentException("Centro no encontrado.");

        if (centro.CentrosHijos.Any())
            throw new ArgumentException("No se puede eliminar el centro porque tiene sub-centros atados a él.");

        // TODO: validacion real transaccional a futuro
        
        _context.CentrosCosto.Remove(centro);
        await _context.SaveChangesAsync();
    }
}
