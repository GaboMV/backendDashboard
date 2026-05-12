using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Domain.Entities;

namespace Sicoin.Contabilidad.Application.Services;

public class ParametrizacionContableService : IParametrizacionContableService
{
    private readonly IContabilidadDbContext _context;

    public ParametrizacionContableService(IContabilidadDbContext context)
    {
        _context = context;
    }

    public async Task<List<GrupoParametrosDto>> ObtenerParametrosAgrupadosAsync(Guid empresaId)
    {
        var parametros = await _context.ParametrosContables
            .Where(p => p.EmpresaId == empresaId)
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.Clave)
            .ToListAsync();

        var mappers = parametros.Select(p => new ParametroContableDto
        {
            ParametroId = p.ParametroId,
            Clave = p.Clave,
            Modulo = p.Modulo,
            Descripcion = p.Descripcion,
            Obligatoriedad = p.Obligatoriedad,
            CodigoCuenta = p.CodigoCuenta,
            NombreCuenta = p.NombreCuenta,
            TipoCuentaEsperado = p.TipoCuentaEsperado
        }).ToList();

        var agrupados = mappers.GroupBy(p => p.Modulo).Select(g =>
        {
            var icono = g.Key switch
            {
                "Compras" => "ri-shopping-cart-line",
                "Ventas" => "ri-price-tag-3-line",
                "Inventarios" => "ri-stack-line",
                "Tesoreria" => "ri-bank-card-line",
                "ContabilidadGeneral" => "ri-book-2-line",
                _ => "ri-settings-2-line"
            };

            return new GrupoParametrosDto
            {
                Modulo = g.Key,
                IconoRemix = icono,
                Parametros = g.ToList()
            };
        }).ToList();

        return agrupados;
    }

    public async Task ActualizarParametroAsync(Guid empresaId, string clave, string codigoCuenta)
    {
        var parametro = await _context.ParametrosContables
            .FirstOrDefaultAsync(p => p.EmpresaId == empresaId && p.Clave == clave);

        if (parametro == null)
            throw new ArgumentException("Parámetro no encontrado");

        if (string.IsNullOrEmpty(codigoCuenta))
        {
            parametro.CodigoCuenta = null;
            parametro.NombreCuenta = null;
        }
        else
        {
            var cuenta = await _context.PlanesCuentas
                .FirstOrDefaultAsync(c => c.Codigo == codigoCuenta);

            if (cuenta == null)
                throw new ArgumentException($"La cuenta {codigoCuenta} no existe en el plan de cuentas.");

            parametro.CodigoCuenta = cuenta.Codigo;
            parametro.NombreCuenta = cuenta.Nombre;
        }

        await _context.SaveChangesAsync();
    }

    public async Task ActualizarParametrosMasivoAsync(Guid empresaId, List<ParametroContableDto> parametros)
    {
        foreach (var p in parametros)
        {
            await ActualizarParametroAsync(empresaId, p.Clave, p.CodigoCuenta ?? "");
        }
    }

    public async Task ConfigurarValoresInicialesSiVacioAsync(Guid empresaId)
    {
        var existe = await _context.ParametrosContables.AnyAsync(p => p.EmpresaId == empresaId);
        if (existe) return;

        var iniciales = new List<ParametroContable>
        {
            // Compras
            new() { EmpresaId = empresaId, Clave = "INVENTARIO_MERCANCIAS", Modulo = "Compras", Descripcion = "Cuenta de inventario por defecto", Obligatoriedad = "Si", TipoCuentaEsperado = "Activo" },
            new() { EmpresaId = empresaId, Clave = "IVA_CREDITO_FISCAL_LOCAL", Modulo = "Compras", Descripcion = "IVA de compras locales", Obligatoriedad = "Si", TipoCuentaEsperado = "Activo" },
            new() { EmpresaId = empresaId, Clave = "CXP_PROVEEDORES_LOCAL", Modulo = "Compras", Descripcion = "Cuenta por pagar proveedores", Obligatoriedad = "Si", TipoCuentaEsperado = "Pasivo" },
            new() { EmpresaId = empresaId, Clave = "MERCANCIAS_EN_TRANSITO", Modulo = "Compras", Descripcion = "Cuenta transitoria para recepciones", Obligatoriedad = "No", TipoCuentaEsperado = "Activo" },
            
            // Ventas
            new() { EmpresaId = empresaId, Clave = "VENTAS_MERCANCIAS", Modulo = "Ventas", Descripcion = "Ingresos por ventas de mercadería", Obligatoriedad = "Si", TipoCuentaEsperado = "Ingreso" },
            new() { EmpresaId = empresaId, Clave = "IVA_DEBITO_FISCAL", Modulo = "Ventas", Descripcion = "IVA cobrado en ventas (13%)", Obligatoriedad = "Si", TipoCuentaEsperado = "Pasivo" },
            new() { EmpresaId = empresaId, Clave = "IT_POR_PAGAR", Modulo = "Ventas", Descripcion = "Impuesto a las Transacciones 3%", Obligatoriedad = "Si", TipoCuentaEsperado = "Pasivo" },
            new() { EmpresaId = empresaId, Clave = "CXC_CLIENTES", Modulo = "Ventas", Descripcion = "Cuentas por cobrar para ventas", Obligatoriedad = "Si", TipoCuentaEsperado = "Activo" },
            new() { EmpresaId = empresaId, Clave = "COSTO_VENTAS", Modulo = "Ventas", Descripcion = "Salida de inventario al confirmar venta", Obligatoriedad = "Si", TipoCuentaEsperado = "Costo" },
            
            // Inventarios
            new() { EmpresaId = empresaId, Clave = "SOBRANTES_INVENTARIO", Modulo = "Inventarios", Descripcion = "Para ajustes de sobrante", Obligatoriedad = "Si", TipoCuentaEsperado = "Ingreso" },
            new() { EmpresaId = empresaId, Clave = "MERMAS_DESMEDROS", Modulo = "Inventarios", Descripcion = "Para ajustes de faltante", Obligatoriedad = "Si", TipoCuentaEsperado = "Gasto" },
            
            // Tesoreria
            new() { EmpresaId = empresaId, Clave = "CAJA_GENERAL", Modulo = "Tesoreria", Descripcion = "Pagos y cobros en efectivo", Obligatoriedad = "Si", TipoCuentaEsperado = "Activo" },
            new() { EmpresaId = empresaId, Clave = "BANCO_PRINCIPAL", Modulo = "Tesoreria", Descripcion = "Transferencias y cheques", Obligatoriedad = "Si", TipoCuentaEsperado = "Activo" },
            
            // General
            new() { EmpresaId = empresaId, Clave = "RESULTADO_EJERCICIO", Modulo = "ContabilidadGeneral", Descripcion = "Resultado neto al cierre", Obligatoriedad = "Si", TipoCuentaEsperado = "Patrimonio" },
            new() { EmpresaId = empresaId, Clave = "RESULTADOS_ACUMULADOS", Modulo = "ContabilidadGeneral", Descripcion = "Resultados retenidos", Obligatoriedad = "Si", TipoCuentaEsperado = "Patrimonio" }
        };

        foreach(var p in iniciales) {
            p.FechaRegistro = DateTime.UtcNow;
            p.UsuarioRegistroId = 1;
            p.EstadoId = "AC";
        }

        _context.ParametrosContables.AddRange(iniciales);
        await _context.SaveChangesAsync();
    }
}
