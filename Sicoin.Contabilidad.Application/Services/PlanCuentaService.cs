using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Domain.Entities;

namespace Sicoin.Contabilidad.Application.Services;

public class PlanCuentaService : IPlanCuentaService
{
    private readonly IContabilidadDbContext _context;

    public PlanCuentaService(IContabilidadDbContext context)
    {
        _context = context;
    }

    public async Task<PlanCuenta> CreateAsync(PlanCuenta account)
    {
        // V-008: Nombre obligatorio
        if (string.IsNullOrWhiteSpace(account.Nombre))
            throw new Exception("El nombre de la cuenta es obligatorio.");

        // V-001: Código único
        if (await _context.PlanesCuentas.AnyAsync(p => p.Codigo == account.Codigo && p.EmpresaId == account.EmpresaId))
            throw new Exception($"El código {account.Codigo} ya está registrado.");

        // Hierarchy logic
        if (account.PlanPadreId.HasValue)
        {
            var padre = await _context.PlanesCuentas.FindAsync(account.PlanPadreId.Value);
            if (padre == null)
                throw new Exception("La cuenta padre no existe.");
            
            // V-009: Cuenta padre activa
            if (padre.EstadoId != "AC")
                throw new Exception("No se puede crear una subcuenta bajo una cuenta inactiva.");

            // V-002: Código coherente con padre
            if (!account.Codigo.StartsWith(padre.Codigo + "."))
                throw new Exception($"El código debe ser hijo del padre seleccionado ({padre.Codigo}).");

            // V-005: Cuenta padre no puede ser de movimiento
            if (padre.AceptaMovimiento)
                throw new Exception("No se puede crear subcuentas bajo una cuenta de movimiento.");

            account.Nivel = padre.Nivel + 1;
        }
        else
        {
            account.Nivel = 1;
        }

        // V-006: Nivel máximo 8
        if (account.Nivel > 8)
            throw new Exception("El sistema permite hasta 8 niveles de profundidad.");

        // Automatic Saldo Normal Calculation
        account.SaldoNormalDescripcion = account.TipoCuentaId switch
        {
            1 or 5 or 6 => "Deudora",
            2 or 3 or 4 => "Acreedora",
            _ => "Otras"
        };
        account.SaldoNormalId = account.SaldoNormalDescripcion == "Deudora" ? 1 : 2;

        _context.PlanesCuentas.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<List<PlanCuentaDto>> GetTreeAsync(Guid empresaId, string? filter = null)
    {
        var query = _context.PlanesCuentas
            .Where(p => p.EmpresaId == empresaId);

        var allAccounts = await query
            .OrderBy(p => p.Codigo)
            .ToListAsync();

        IEnumerable<PlanCuenta> filteredAccounts = allAccounts;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var search = filter.ToLower();
            var matches = allAccounts.Where(a => 
                a.Nombre.ToLower().Contains(search) || 
                a.Codigo.ToLower().Contains(search))
                .ToList();

            // Trace up to include all ancestors of matching accounts
            var includedIds = new HashSet<long>();
            foreach (var match in matches)
            {
                var current = match;
                while (current != null)
                {
                    if (!includedIds.Add(current.PlanId)) break; // Already included
                    current = current.PlanPadreId.HasValue 
                        ? allAccounts.FirstOrDefault(a => a.PlanId == current.PlanPadreId.Value) 
                        : null;
                }
            }
            filteredAccounts = allAccounts.Where(a => includedIds.Contains(a.PlanId));
        }

        var dtos = filteredAccounts.Select(a => new PlanCuentaDto
        {
            PlanId = a.PlanId,
            Codigo = a.Codigo,
            Nombre = a.Nombre,
            Nivel = a.Nivel,
            AceptaMovimiento = a.AceptaMovimiento,
            TipoCuentaDescripcion = a.TipoCuentaDescripcion,
            SaldoNormalDescripcion = a.SaldoNormalDescripcion
        }).ToList();

        var dict = dtos.ToDictionary(d => d.PlanId);
        var rootNodes = new List<PlanCuentaDto>();

        foreach (var dto in dtos)
        {
            var original = allAccounts.First(a => a.PlanId == dto.PlanId);
            if (original.PlanPadreId.HasValue && dict.TryGetValue(original.PlanPadreId.Value, out var parentDto))
            {
                parentDto.Children.Add(dto);
            }
            else
            {
                rootNodes.Add(dto);
            }
        }

        return rootNodes;
    }

    public async Task<PlanCuentaDetailDto> GetByIdAsync(long planId, Guid empresaId)
    {
        var account = await _context.PlanesCuentas
            .FirstOrDefaultAsync(p => p.PlanId == planId && p.EmpresaId == empresaId);

        if (account == null)
            return null!;

        string padreCodigoNombre = string.Empty;
        if (account.PlanPadreId.HasValue)
        {
            var padre = await _context.PlanesCuentas.FindAsync(account.PlanPadreId.Value);
            padreCodigoNombre = padre != null ? $"{padre.Codigo} — {padre.Nombre}" : string.Empty;
        }

        // Calculate balance using ComprobanteDetalles
        var movimientosQuery = _context.ComprobanteDetalles
            .Include(d => d.Comprobante)
            .Where(d => d.CuentaId == planId && d.Comprobante.EstadoComprobanteId == 2);

        var totalDebe = await movimientosQuery.SumAsync(d => d.Debe);
        var totalHaber = await movimientosQuery.SumAsync(d => d.Haber);
        var movimientoCount = await movimientosQuery.CountAsync();

        // Saldo Actual based on Naturaleza
        var saldoActual = (account.TipoCuentaId == 1 || account.TipoCuentaId == 5 || account.TipoCuentaId == 6)
            ? totalDebe - totalHaber
            : totalHaber - totalDebe;

        return new PlanCuentaDetailDto
        {
            PlanId = account.PlanId,
            PlanPadreId = account.PlanPadreId,
            PlanPadreCodigoNombre = padreCodigoNombre,
            EmpresaId = account.EmpresaId,
            Codigo = account.Codigo,
            Nombre = account.Nombre,
            Nivel = account.Nivel,
            TipoCuentaId = account.TipoCuentaId,
            SaldoNormalId = account.SaldoNormalId,
            SaldoNormalDescripcion = account.SaldoNormalDescripcion,
            AceptaMovimiento = account.AceptaMovimiento,
            AceptaMoneda = account.AceptaMoneda,
            RequiereCosto = account.RequiereCosto,
            RequiereProyecto = account.RequiereProyecto,
            CodigoSinNandina = account.CodigoSinNandina,
            EstadoId = account.EstadoId,
            Observacion = account.Observacion,
            SaldoActual = saldoActual,
            MovimientoCount = movimientoCount
        };
    }

    public async Task<PlanCuenta> UpdateAsync(PlanCuenta account)
    {
        var existing = await _context.PlanesCuentas
            .FirstOrDefaultAsync(p => p.PlanId == account.PlanId && p.EmpresaId == account.EmpresaId);

        if (existing == null)
            throw new Exception("La cuenta no existe.");

        // V-008: Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(account.Nombre))
            throw new Exception("El nombre de la cuenta es obligatorio.");

        bool hasConfirmedMovements = await _context.ComprobanteDetalles
            .Include(d => d.Comprobante)
            .AnyAsync(d => d.CuentaId == account.PlanId && d.Comprobante.EstadoComprobanteId == 2);

        // V-003: TipoCuenta inmutable si tiene movimientos confirmados
        if (existing.TipoCuentaId != account.TipoCuentaId && hasConfirmedMovements)
        {
            throw new Exception("No se puede cambiar el tipo contable de una cuenta que tiene movimientos confirmados.");
        }

        // V-004: Desactivar cuenta con saldo
        if (existing.EstadoId == "AC" && account.EstadoId == "IN")
        {
            var movimientosQuery = _context.ComprobanteDetalles
                .Include(d => d.Comprobante)
                .Where(d => d.CuentaId == account.PlanId && d.Comprobante.EstadoComprobanteId == 2);
            
            var debe = await movimientosQuery.SumAsync(d => d.Debe);
            var haber = await movimientosQuery.SumAsync(d => d.Haber);
            
            // Si el saldo no es cero, bloqueamos
            if (debe != haber)
                throw new Exception($"La cuenta tiene saldo o movimientos (Debe: Bs {debe}, Haber: Bs {haber}). No se puede desactivar.");
        }

        // V-005: Cambio de agrupación a movimiento
        if (!existing.AceptaMovimiento && account.AceptaMovimiento)
        {
            bool hasChildren = await _context.PlanesCuentas.AnyAsync(p => p.PlanPadreId == account.PlanId);
            if (hasChildren)
                throw new Exception("No se puede marcar como cuenta de movimiento porque tiene subcuentas asociadas.");
        }

        existing.Nombre = account.Nombre;
        existing.TipoCuentaId = account.TipoCuentaId;
        existing.AceptaMovimiento = account.AceptaMovimiento;
        existing.AceptaMoneda = account.AceptaMoneda;
        existing.RequiereCosto = account.RequiereCosto;
        existing.RequiereProyecto = account.RequiereProyecto;
        existing.CodigoSinNandina = account.CodigoSinNandina;
        existing.Observacion = account.Observacion;
        existing.EstadoId = account.EstadoId;

        _context.PlanesCuentas.Update(existing);
        await _context.SaveChangesAsync();

        return existing;
    }
}
