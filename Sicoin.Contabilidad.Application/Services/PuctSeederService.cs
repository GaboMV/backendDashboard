using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Domain.Entities;
using Sicoin.Contabilidad.Application.Interfaces;

namespace Sicoin.Contabilidad.Application.Services;

public class PuctSeederService
{
    private readonly IContabilidadDbContext _context;

    public PuctSeederService(IContabilidadDbContext context)
    {
        _context = context;
    }

    public async Task SeedPuctOficialAsync(Guid empresaId)
    {
        bool existe = await _context.PlanesCuentas.AnyAsync(x => x.EmpresaId == empresaId);
        if (existe) return; // Ya se ha sembrado o la empresa tiene datos

        var nodes = new List<PlanCuenta>
        {
            // --- 1. ACTIVO ---
            Create(empresaId, null, "1", "ACTIVO", 1, 1, "ACTIVO", 1, "Deudora"),
            Create(empresaId, "1", "1.1", "ACTIVO CORRIENTE", 2, 1, "ACTIVO", 1, "Deudora"),
            Create(empresaId, "1.1", "1.1.01", "EFECTIVO Y EQUIVALENTES DE EFECTIVO", 3, 1, "ACTIVO", 1, "Deudora"),
            Create(empresaId, "1.1.01", "1.1.01.001", "CAJA", 4, 1, "ACTIVO", 1, "Deudora"),
            Create(empresaId, "1.1.01", "1.1.01.002", "BANCOS", 4, 1, "ACTIVO", 1, "Deudora"),
            Create(empresaId, "1.1", "1.1.02", "COBROS CORTOS", 3, 1, "ACTIVO", 1, "Deudora"),
            Create(empresaId, "1.1.02", "1.1.02.001", "DOCUMENTOS POR COBRAR", 4, 1, "ACTIVO", 1, "Deudora"),

            Create(empresaId, "1", "1.2", "ACTIVO NO CORRIENTE", 2, 1, "ACTIVO", 1, "Deudora"),
            Create(empresaId, "1.2", "1.2.01", "PROPIEDAD PLANTA Y EQUIPOS", 3, 1, "ACTIVO", 1, "Deudora"),
            Create(empresaId, "1.2.01", "1.2.01.001", "TERRENOS", 4, 1, "ACTIVO", 1, "Deudora"),
            Create(empresaId, "1.2.01", "1.2.01.002", "EDIFICOS", 4, 1, "ACTIVO", 1, "Deudora"),

            // --- 2. PASIVO ---
            Create(empresaId, null, "2", "PASIVO", 1, 2, "PASIVO", 2, "Acreedora"),
            Create(empresaId, "2", "2.1", "PASIVO CORRIENTE", 2, 2, "PASIVO", 2, "Acreedora"),
            Create(empresaId, "2.1", "2.1.01", "CUENTAS POR PAGAR", 3, 2, "PASIVO", 2, "Acreedora"),
            Create(empresaId, "2.1.01", "2.1.01.001", "PROVEEDORES LOCALES", 4, 2, "PASIVO", 2, "Acreedora"),
            Create(empresaId, "2.1.01", "2.1.01.002", "PROVEEDORES EXTERIOR", 4, 2, "PASIVO", 2, "Acreedora"),

            // --- 3. PATRIMONIO ---
            Create(empresaId, null, "3", "PATRIMONIO", 1, 3, "PATRIMONIO", 2, "Acreedora"),
            Create(empresaId, "3", "3.1", "CAPITAL", 2, 3, "PATRIMONIO", 2, "Acreedora"),
            Create(empresaId, "3.1", "3.1.01", "CAPITAL SOCIAL", 3, 3, "PATRIMONIO", 2, "Acreedora"),
            Create(empresaId, "3.1.01", "3.1.01.001", "CAPITAL PAGADO", 4, 3, "PATRIMONIO", 2, "Acreedora"),

            // --- 4. INGRESOS ---
            Create(empresaId, null, "4", "INGRESOS", 1, 4, "INGRESOS", 2, "Acreedora"),
            Create(empresaId, "4", "4.1", "INGRESOS OPERATIVOS", 2, 4, "INGRESOS", 2, "Acreedora"),
            Create(empresaId, "4.1", "4.1.01", "VENTAS", 3, 4, "INGRESOS", 2, "Acreedora"),
            Create(empresaId, "4.1.01", "4.1.01.001", "VENTAS LOCALES", 4, 4, "INGRESOS", 2, "Acreedora"),

            // --- 5. EGRESOS / COSTOS ---
            Create(empresaId, null, "5", "COSTOS", 1, 5, "COSTOS", 1, "Deudora"),
            Create(empresaId, "5", "5.1", "COSTOS OPERATIVOS", 2, 5, "COSTOS", 1, "Deudora"),
            Create(empresaId, "5.1", "5.1.01", "COSTO DE VENTAS", 3, 5, "COSTOS", 1, "Deudora"),
            Create(empresaId, "5.1.01", "5.1.01.001", "COSTO VENTAS LOCALES", 4, 5, "COSTOS", 1, "Deudora"),

            // --- 6. GASTOS ---
            Create(empresaId, null, "6", "GASTOS", 1, 6, "GASTOS", 1, "Deudora"),
            Create(empresaId, "6", "6.1", "GASTOS OPERATIVOS", 2, 6, "GASTOS", 1, "Deudora"),
            Create(empresaId, "6.1", "6.1.01", "GASTOS ADMINISTRATIVOS", 3, 6, "GASTOS", 1, "Deudora"),
            Create(empresaId, "6.1.01", "6.1.01.001", "SUELDOS Y SALARIOS", 4, 6, "GASTOS", 1, "Deudora"),
            Create(empresaId, "6.1.01", "6.1.01.002", "SERVICIOS BASICOS", 4, 6, "GASTOS", 1, "Deudora")
        };

        var dict = new Dictionary<string, PlanCuenta>();

        foreach (var node in nodes)
        {
            if (node.PlanPadreId == null) // This field is technically PlanPadreId long, which we don't have yet.
            {
                // Root node logic
            }
        }

        // To assign sequential long IDs to the parents, we must insert them sequentially or use Navigation properties if we construct the tree.
        // It's easier to just SaveChanges on each level.

        for (int nivel = 1; nivel <= 4; nivel++)
        {
            var cuentasNivel = nodes.Where(x => x.Nivel == nivel).ToList();
            foreach (var cuenta in cuentasNivel)
            {
                // Asignar PadreId buscando la cuenta padre guardada en DB (niveles anteriores ya tienen ID real)
                if (nivel > 1)
                {
                    var parentCode = cuenta.Codigo.Substring(0, cuenta.Codigo.LastIndexOf('.'));
                    var parentDb = await _context.PlanesCuentas.FirstAsync(p => p.Codigo == parentCode && p.EmpresaId == empresaId);
                    cuenta.PlanPadreId = parentDb.PlanId;
                }
                
                _context.PlanesCuentas.Add(cuenta);
                await _context.SaveChangesAsync();
            }
        }
    }

    private PlanCuenta Create(Guid empresaId, string? padreCod, string codigo, string nombre, int nivel, int tcId, string tcDesc, int snId, string snDesc)
    {
        return new PlanCuenta
        {
            EmpresaId = empresaId,
            Codigo = codigo,
            Nombre = nombre,
            Nivel = nivel,
            TipoCuentaId = tcId,
            TipoCuentaDescripcion = tcDesc,
            SaldoNormalId = snId,
            SaldoNormalDescripcion = snDesc,
            AceptaMovimiento = false, // Levels 1-4 are NEVER movimiento on PUCT
            RequiereCosto = false,
            AceptaMoneda = false,
            EstadoId = "AC"
        };
    }
}
