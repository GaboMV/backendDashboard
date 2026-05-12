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
        // V-008: Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(account.Nombre))
            throw new Exception("El nombre de la cuenta analítica es obligatorio.");

        // V-008: Cuenta de Ajuste
        if (account.AceptaMoneda && !account.CuentaAjusteId.HasValue)
            throw new Exception("La cuenta acepta moneda extranjera pero no tiene Cuenta de Ajuste por Diferencia de Cambio configurada.");

        // Hierarchy logic
        if (account.PlanPadreId.HasValue)
        {
            var padre = await _context.PlanesCuentas.FindAsync(account.PlanPadreId.Value);
            if (padre == null)
                throw new Exception("La cuenta padre no existe.");
            
            // V-009: Niveles inmutables - la empresa solo puede crear CAs bajo un CP (nivel 4) u otros CA
            if (padre.Nivel < 4)
                throw new Exception("No puede crear una cuenta directamente bajo una Clase, Grupo o Subgrupo. Seleccione una Cuenta Principal (Nivel 4).");

            account.Nivel = padre.Nivel + 1;
            
            // V-001 / V-010: Validate full code format
            if (!account.Codigo.StartsWith($"{padre.Codigo}."))
                throw new Exception($"El código debe comenzar con el código del padre ({padre.Codigo}.).");

            var suffix = account.Codigo.Substring(padre.Codigo.Length + 1);
            var isSequence = suffix.All(char.IsDigit) || suffix.EndsWith("N");
            if (!isSequence) throw new Exception("El segmento final del código solo acepta dígitos o el valor especial '00N'.");
            
            // V-001: Código único re-check con prefijo asignado
            if (await _context.PlanesCuentas.AnyAsync(p => p.Codigo == account.Codigo && p.EmpresaId == account.EmpresaId))
                throw new Exception($"El código {account.Codigo} ya está registrado bajo {padre.Nombre}.");
        }
        else
        {
            throw new Exception("No se pueden crear Cuentas de Nivel 1. Use la estructura oficial PUCT.");
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
            TipoCuentaId = a.TipoCuentaId,
            TipoCuentaDescripcion = a.TipoCuentaDescripcion,
            SaldoNormalId = a.SaldoNormalId,
            SaldoNormalDescripcion = a.SaldoNormalDescripcion,
            PlanPadreId = a.PlanPadreId,
            EstadoId = a.EstadoId
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

        // V-009: Nodos inmutables
        if (existing.Nivel < 5)
            throw new Exception("Este nivel pertenece a la estructura oficial del PUCT SIN Bolivia y no puede modificarse.");

        // V-008: Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(account.Nombre))
            throw new Exception("El nombre de la cuenta es obligatorio.");

        // V-008: Cuenta de Ajuste requerida
        if (account.AceptaMoneda && !account.CuentaAjusteId.HasValue)
            throw new Exception("La cuenta acepta moneda extranjera pero no tiene Cuenta de Ajuste por Diferencia de Cambio configurada.");

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
        existing.CuentaAjusteId = account.CuentaAjusteId;
        existing.EstadoId = account.EstadoId;

        _context.PlanesCuentas.Update(existing);
        await _context.SaveChangesAsync();

        return existing;
    }
    public async Task<ImportacionResultadoDto> ImportarCuentasAnaliticasAsync(System.IO.Stream excelStream, Guid empresaId, bool soloValidar, bool sobreescribir)
    {
        var result = new ImportacionResultadoDto();
        
        using var workbook = new ClosedXML.Excel.XLWorkbook(excelStream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RowsUsed().Skip(1); // skip header

        var groupedAccounts = await _context.PlanesCuentas
            .Where(p => p.EmpresaId == empresaId)
            .ToListAsync();
            
        var CPs = groupedAccounts.Where(p => p.Nivel == 4).ToDictionary(p => p.Codigo);
        var CAs = groupedAccounts.Where(p => p.Nivel == 5).ToDictionary(p => p.Codigo);

        foreach (var row in rows)
        {
            result.TotalFilasLeidas++;
            int index = row.RowNumber();

            var valC = row.Cell(1).GetString().Trim();
            var valG = row.Cell(2).GetString().Trim();
            var valSG = row.Cell(3).GetString().Trim();
            var valCP = row.Cell(4).GetString().Trim();
            var valCA = row.Cell(5).GetString().Trim();
            var nombre = row.Cell(6).GetString().Trim();
            var reqCosto = row.Cell(7).GetString().Trim() == "1";
            var aceptaMon = row.Cell(8).GetString().Trim() == "1";
            var obs = row.Cell(9).GetString().Trim();

            // Validación V-006 Básica
            if (string.IsNullOrEmpty(valC) || string.IsNullOrEmpty(valCA))
            {
                result.Errores++;
                result.Detalles.Add(new ImportacionDetalleDto { Fila = index, Tipo = "ERROR", Mensaje = "Fila mal formada o vacía." });
                continue;
            }

            // Validación V-010: Formato de Nùmero CA
            var isSequenceCA = valCA.All(char.IsDigit) || valCA.EndsWith("N");
            if (!isSequenceCA)
            {
                result.Errores++;
                result.Detalles.Add(new ImportacionDetalleDto { Fila = index, Tipo = "ERROR", Mensaje = $"El número CA '{valCA}' no es válido. Solo dígitos o '00N'." });
                continue;
            }

            // V-004: Clase
            if (!new[] { "1", "2", "3", "4", "5" }.Contains(valC))
            {
                result.Errores++;
                result.Detalles.Add(new ImportacionDetalleDto { Fila = index, Tipo = "ERROR", Mensaje = $"Clase {valC} no válida. Use 1 a 5." });
                continue;
            }

            // V-003: Nombre Vacío
            if (string.IsNullOrEmpty(nombre))
            {
                result.Errores++;
                result.Detalles.Add(new ImportacionDetalleDto { Fila = index, Tipo = "ERROR", Mensaje = "El nombre de la CA es obligatorio." });
                continue;
            }

            // Construir código padre
            var codPadre = $"{valC}.{valG}.{valSG}.{valCP}";
            
            // V-001: CP no existe en el PUCT
            if (!CPs.TryGetValue(codPadre, out var cpParent))
            {
                result.Errores++;
                result.Detalles.Add(new ImportacionDetalleDto { Fila = index, Tipo = "ERROR", Mensaje = $"La Cuenta Principal {codPadre} no existe en el PUCT." });
                continue;
            }

            // Códig CA
            var codCA = $"{codPadre}.{valCA}";

            if (CAs.TryGetValue(codCA, out var caExistente))
            {
                if (!sobreescribir)
                {
                    result.Omitidas++;
                    result.Detalles.Add(new ImportacionDetalleDto { Fila = index, Cuenta = codCA, Tipo = "INFO", Mensaje = "La cuenta ya existe. Se omitió (Sobreescribir = No)." });
                }
                else
                {
                    if (!soloValidar)
                    {
                        caExistente.Nombre = nombre;
                        caExistente.RequiereCosto = reqCosto;
                        caExistente.AceptaMoneda = aceptaMon;
                        caExistente.Observacion = obs;
                        _context.PlanesCuentas.Update(caExistente);
                    }
                    result.Actualizadas++;
                    result.Detalles.Add(new ImportacionDetalleDto { Fila = index, Cuenta = codCA, Tipo = "SUCCESS", Mensaje = "Actualizada correctamente." });
                }
            }
            else
            {
                if (!soloValidar)
                {
                    var nuevaCuenta = new PlanCuenta
                    {
                        EmpresaId = empresaId,
                        PlanPadreId = cpParent.PlanId,
                        Codigo = codCA,
                        Nombre = nombre,
                        Nivel = 5,
                        AceptaMovimiento = true,
                        TipoCuentaId = cpParent.TipoCuentaId,
                        TipoCuentaDescripcion = cpParent.TipoCuentaDescripcion,
                        SaldoNormalId = cpParent.SaldoNormalId,
                        SaldoNormalDescripcion = cpParent.SaldoNormalDescripcion,
                        RequiereCosto = reqCosto,
                        AceptaMoneda = aceptaMon,
                        Observacion = obs
                    };
                    _context.PlanesCuentas.Add(nuevaCuenta);
                    CAs.Add(codCA, nuevaCuenta); // Add to cache for duplicate detection in same loop
                }
                result.Creadas++;
                result.Detalles.Add(new ImportacionDetalleDto { Fila = index, Cuenta = codCA, Tipo = "SUCCESS", Mensaje = "Creada correctamente." });
            }
        }

        if (!soloValidar && result.Errores == 0)
        {
            await _context.SaveChangesAsync();
        }

        return result;
    }

    public async Task DeleteAsync(long planId, Guid empresaId)
    {
        var existing = await _context.PlanesCuentas
            .FirstOrDefaultAsync(p => p.PlanId == planId && p.EmpresaId == empresaId);

        if (existing == null)
            throw new Exception("La cuenta no existe.");

        // Rule: Can't delete accounts with children
        bool hasChildren = await _context.PlanesCuentas.AnyAsync(p => p.PlanPadreId == planId);
        if (hasChildren)
            throw new Exception("No se puede eliminar la cuenta porque tiene subcuentas asociadas. Elimine primero sus hijos.");

        // Rule: Can't delete accounts with movements
        bool hasMovements = await _context.ComprobanteDetalles.AnyAsync(d => d.CuentaId == planId);
        if (hasMovements)
            throw new Exception("No se puede eliminar la cuenta porque ya registra movimientos contables.");

        _context.PlanesCuentas.Remove(existing);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PlanCuentaDto>> SearchCuentasMovimientoAsync(Guid empresaId, string query)
    {
        var q = query.ToLower();
        var accounts = await _context.PlanesCuentas
            // .Where(p => p.EmpresaId == empresaId && p.EstadoId == "AC") // Temporarily disabled for diagnosis
            .Where(p => p.Codigo.ToLower().Contains(q) || p.Nombre.ToLower().Contains(q))
            .OrderBy(p => p.Codigo)
            .Take(20)
            .Select(a => new PlanCuentaDto
            {
                PlanId = a.PlanId,
                Codigo = a.Codigo,
                Nombre = a.Nombre,
                AceptaMovimiento = a.AceptaMovimiento, // We return the flag so UI can warn if needed
                TipoCuentaDescripcion = a.TipoCuentaDescripcion,
                SaldoNormalDescripcion = a.SaldoNormalDescripcion,
                EstadoId = a.EstadoId
            })
            .ToListAsync();

        return accounts;
    }

    public async Task<List<PlanCuentaDto>> GetCuentasConCentroCostoAsync(Guid empresaId)
    {
        return await _context.PlanesCuentas
            .Where(p => p.EmpresaId == empresaId && p.RequiereCosto && p.EstadoId == "AC")
            .OrderBy(p => p.Codigo)
            .Select(a => new PlanCuentaDto
            {
                PlanId = a.PlanId,
                Codigo = a.Codigo,
                Nombre = a.Nombre,
                AceptaMovimiento = a.AceptaMovimiento,
                TipoCuentaDescripcion = a.TipoCuentaDescripcion,
                SaldoNormalDescripcion = a.SaldoNormalDescripcion,
                EstadoId = a.EstadoId
            })
            .ToListAsync();
    }
}
