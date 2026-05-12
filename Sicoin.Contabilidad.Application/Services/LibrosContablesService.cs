using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Application.Services;

public class LibrosContablesService : ILibrosContablesService
{
    private readonly IContabilidadDbContext _db;

    public LibrosContablesService(IContabilidadDbContext db)
    {
        _db = db;
    }

    // ─────────────────────────────────────────────
    // LIBRO DIARIO
    // ─────────────────────────────────────────────
    public async Task<LibroDiarioResponseDto> GetLibroDiarioAsync(
        Guid empresaId,
        int gestion,
        int mesDesde,
        int mesHasta,
        List<int>? tiposComprobante = null,
        bool incluirAnulados = false)
    {
        var query = _db.Comprobantes
            .Include(c => c.Detalles)
                .ThenInclude(d => d.PlanCuenta)
            .Where(c => c.EmpresaId == empresaId
                     && c.Gestion == gestion
                     && c.PeriodoMes >= mesDesde
                     && c.PeriodoMes <= mesHasta);

        // Solo mostrar Confirmados (y Anulados si se pide)
        if (incluirAnulados)
            query = query.Where(c => c.EstadoComprobanteId == 2 || c.EstadoComprobanteId == 3);
        else
            query = query.Where(c => c.EstadoComprobanteId == 2); // solo Confirmados

        if (tiposComprobante != null && tiposComprobante.Any())
            query = query.Where(c => tiposComprobante.Contains(c.TipoComprobanteId));

        var comprobantes = await query
            .OrderBy(c => c.FechaContable)
            .ThenBy(c => c.NroComprobante)
            .ToListAsync();

        var comprobantesDto = comprobantes.Select(c => new ComprobanteDiarioDto
        {
            Id            = c.ComprobanteId,
            Numero        = c.NroComprobante,
            Fecha         = c.FechaContable,
            Tipo          = c.TipoComprobanteDescripcion,
            Concepto      = c.Concepto,
            ReferenciaExterna = c.ReferenciaExterna,
            TotalDebe     = c.TotalDebe,
            TotalHaber    = c.TotalHaber,
            Estado        = c.EstadoComprobanteDescripcion,
            EsAutomatico  = c.EsAutomatico,
            Lineas        = c.Detalles
                .OrderBy(d => d.NroLinea)
                .Select(d => new LineaComprobanteDiarioDto
                {
                    NroLinea      = d.NroLinea,
                    CodigoCuenta  = d.PlanCuenta?.Codigo ?? "",
                    NombreCuenta  = d.PlanCuenta?.Nombre ?? "",
                    CentroCosto   = d.AuxiliarReferencia, // se puede enriquecer con el nombre del CC
                    Glosa         = d.Glosa,
                    Debe          = d.Debe,
                    Haber         = d.Haber
                }).ToList()
        }).ToList();

        // Solo sumar los NO anulados para los totales del pie
        var confirmados = comprobantesDto.Where(c => c.Estado != "ANULADO" && c.Estado != "Anulado").ToList();

        return new LibroDiarioResponseDto
        {
            Gestion           = gestion,
            MesDesde          = mesDesde,
            MesHasta          = mesHasta,
            Comprobantes      = comprobantesDto,
            TotalDebeGeneral  = confirmados.Sum(c => c.TotalDebe),
            TotalHaberGeneral = confirmados.Sum(c => c.TotalHaber)
        };
    }

    // ─────────────────────────────────────────────
    // LIBRO MAYOR
    // ─────────────────────────────────────────────
    public async Task<LibroMayorResponseDto> GetLibroMayorAsync(
        Guid empresaId,
        List<long> cuentaIds,
        int gestion,
        int mesDesde,
        int mesHasta)
    {
        // Traer todas las cuentas pedidas
        var cuentas = await _db.PlanesCuentas
            .Where(p => cuentaIds.Contains(p.PlanId) && p.EmpresaId == empresaId)
            .ToListAsync();

        // Traer movimientos del rango
        var movimientos = await _db.ComprobanteDetalles
            .Include(d => d.Comprobante)
            .Where(d => cuentaIds.Contains(d.CuentaId)
                     && d.Comprobante.EmpresaId == empresaId
                     && d.Comprobante.Gestion == gestion
                     && d.Comprobante.PeriodoMes >= mesDesde
                     && d.Comprobante.PeriodoMes <= mesHasta
                     && d.Comprobante.EstadoComprobanteId == 2) // solo confirmados
            .OrderBy(d => d.Comprobante.FechaContable)
            .ThenBy(d => d.Comprobante.NroComprobante)
            .ToListAsync();

        // Saldo inicial = movimientos de periodos anteriores al rango
        var movimientosAnteriores = await _db.ComprobanteDetalles
            .Include(d => d.Comprobante)
            .Where(d => cuentaIds.Contains(d.CuentaId)
                     && d.Comprobante.EmpresaId == empresaId
                     && d.Comprobante.Gestion == gestion
                     && d.Comprobante.PeriodoMes < mesDesde
                     && d.Comprobante.EstadoComprobanteId == 2)
            .ToListAsync();

        var cuentasDto = new List<CuentaMayorDto>();

        foreach (var cuenta in cuentas)
        {
            // Naturaleza según tipo de cuenta
            bool esDeudora = cuenta.TipoCuentaId == 1 || cuenta.TipoCuentaId == 5 || cuenta.TipoCuentaId == 6;
            string naturaleza = esDeudora ? "Deudora" : "Acreedora";

            // Saldo inicial
            var movAnt = movimientosAnteriores.Where(d => d.CuentaId == cuenta.PlanId).ToList();
            decimal saldoInicial = esDeudora
                ? movAnt.Sum(d => d.Debe) - movAnt.Sum(d => d.Haber)
                : movAnt.Sum(d => d.Haber) - movAnt.Sum(d => d.Debe);

            // Movimientos del período
            var movPeriodo = movimientos.Where(d => d.CuentaId == cuenta.PlanId).ToList();
            decimal acumulado = saldoInicial;
            decimal totalDebe = 0, totalHaber = 0;

            var lineas = movPeriodo.Select(d =>
            {
                totalDebe  += d.Debe;
                totalHaber += d.Haber;
                acumulado  += esDeudora ? (d.Debe - d.Haber) : (d.Haber - d.Debe);
                return new MovimientoMayorDto
                {
                    Fecha              = d.Comprobante.FechaContable,
                    NumeroComprobante  = d.Comprobante.NroComprobante,
                    Concepto           = d.Comprobante.Concepto,
                    Glosa              = d.Glosa,
                    Debe               = d.Debe,
                    Haber              = d.Haber,
                    SaldoAcumulado     = acumulado
                };
            }).ToList();

            cuentasDto.Add(new CuentaMayorDto
            {
                CuentaId    = cuenta.PlanId,
                Codigo      = cuenta.Codigo,
                Nombre      = cuenta.Nombre,
                Naturaleza  = naturaleza,
                SaldoInicial = saldoInicial,
                TotalDebe   = totalDebe,
                TotalHaber  = totalHaber,
                SaldoFinal  = acumulado,
                Movimientos = lineas
            });
        }

        return new LibroMayorResponseDto
        {
            Gestion  = gestion,
            MesDesde = mesDesde,
            MesHasta = mesHasta,
            Cuentas  = cuentasDto
        };
    }

    // ─────────────────────────────────────────────
    // BALANCE DE COMPROBACIÓN
    // ─────────────────────────────────────────────
    public async Task<BalanceComprobacionResponseDto> GetBalanceComprobacionAsync(
        Guid empresaId,
        int gestion,
        int mesDesde,
        int mesHasta,
        int nivelMaximo = 4)
    {
        // Traer todas las cuentas de la empresa hasta el nivel pedido
        var cuentas = await _db.PlanesCuentas
            .Where(p => p.EmpresaId == empresaId && p.Nivel <= nivelMaximo)
            .OrderBy(p => p.Codigo)
            .ToListAsync();

        // Movimientos del período (solo confirmados)
        var movPeriodo = await _db.ComprobanteDetalles
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante.EmpresaId == empresaId
                     && d.Comprobante.Gestion == gestion
                     && d.Comprobante.PeriodoMes >= mesDesde
                     && d.Comprobante.PeriodoMes <= mesHasta
                     && d.Comprobante.EstadoComprobanteId == 2)
            .ToListAsync();

        // Movimientos anteriores al rango (para saldo inicial)
        var movAnteriores = await _db.ComprobanteDetalles
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante.EmpresaId == empresaId
                     && d.Comprobante.Gestion == gestion
                     && d.Comprobante.PeriodoMes < mesDesde
                     && d.Comprobante.EstadoComprobanteId == 2)
            .ToListAsync();

        var filas = new List<FilaBalanceDto>();

        foreach (var cuenta in cuentas)
        {
            bool esDeudora = cuenta.TipoCuentaId == 1 || cuenta.TipoCuentaId == 5 || cuenta.TipoCuentaId == 6;

            // Calcular saldo inicial (solo si es cuenta de movimiento o suma hijos para cuentas padre)
            var movAntCuenta = movAnteriores.Where(d => d.CuentaId == cuenta.PlanId).ToList();
            decimal debe0 = movAntCuenta.Sum(d => d.Debe);
            decimal haber0 = movAntCuenta.Sum(d => d.Haber);
            decimal saldoInicial = esDeudora ? debe0 - haber0 : haber0 - debe0;

            // Movimientos del período
            var movCuenta = movPeriodo.Where(d => d.CuentaId == cuenta.PlanId).ToList();
            decimal movDebe  = movCuenta.Sum(d => d.Debe);
            decimal movHaber = movCuenta.Sum(d => d.Haber);
            decimal saldoFinal = saldoInicial + (esDeudora ? movDebe - movHaber : movHaber - movDebe);

            // Solo incluir si tiene algún saldo o movimiento
            if (saldoInicial == 0 && movDebe == 0 && movHaber == 0) continue;

            filas.Add(new FilaBalanceDto
            {
                Codigo               = cuenta.Codigo,
                Nombre               = cuenta.Nombre,
                Nivel                = cuenta.Nivel,
                SaldoDeudorInicial   = esDeudora && saldoInicial > 0 ? saldoInicial : 0,
                SaldoAcreedorInicial = !esDeudora && saldoInicial > 0 ? saldoInicial : 0,
                MovimientosDebe      = movDebe,
                MovimientosHaber     = movHaber,
                SaldoDeudorFinal     = esDeudora && saldoFinal > 0 ? saldoFinal : 0,
                SaldoAcreedorFinal   = !esDeudora && saldoFinal > 0 ? saldoFinal : 0
            });
        }

        // Totales de cuentas de nivel detalle (hoja)
        var detalle = filas.Where(f => f.Nivel == nivelMaximo).ToList();
        decimal sumDI = detalle.Sum(x => x.SaldoDeudorInicial);
        decimal sumAI = detalle.Sum(x => x.SaldoAcreedorInicial);
        decimal sumMD = detalle.Sum(x => x.MovimientosDebe);
        decimal sumMH = detalle.Sum(x => x.MovimientosHaber);
        decimal sumDF = detalle.Sum(x => x.SaldoDeudorFinal);
        decimal sumAF = detalle.Sum(x => x.SaldoAcreedorFinal);

        decimal diferencia = Math.Abs(sumDF - sumAF);

        return new BalanceComprobacionResponseDto
        {
            Gestion               = gestion,
            MesDesde              = mesDesde,
            MesHasta              = mesHasta,
            Filas                 = filas,
            TotalDeudorInicial    = sumDI,
            TotalAcreedorInicial  = sumAI,
            TotalMovimientosDebe  = sumMD,
            TotalMovimientosHaber = sumMH,
            TotalDeudorFinal      = sumDF,
            TotalAcreedorFinal    = sumAF,
            Cuadrado              = diferencia == 0,
            Diferencia            = diferencia
        };
    }
}
