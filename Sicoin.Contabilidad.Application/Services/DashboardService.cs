using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IContabilidadDbContext _context;

    public DashboardService(IContabilidadDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponseDto> GetDashboardSummaryAsync(Guid empresaId, int? sucursalId, int gestion, int periodoMes)
    {
        var response = new DashboardResponseDto();

        // Base query for Comprobantes
        var queryComprobantes = _context.Comprobantes
            .Where(c => c.EmpresaId == empresaId && c.Gestion == gestion);

        if (sucursalId.HasValue)
        {
            queryComprobantes = queryComprobantes.Where(c => c.SucursalId == sucursalId.Value);
        }

        // Voucher Summary for the requested period
        var periodVouchers = await queryComprobantes
            .Where(c => c.PeriodoMes == periodoMes)
            .ToListAsync();

        response.VouchersCounts.Confirmados = periodVouchers.Count(c => c.EstadoComprobanteId == 2);
        response.VouchersCounts.EnBorrador = periodVouchers.Count(c => c.EstadoComprobanteId == 1);
        response.VouchersCounts.Anulados = periodVouchers.Count(c => c.EstadoComprobanteId == 3);
        response.VouchersCounts.Automaticos = periodVouchers.Count(c => c.EsAutomatico);
        response.VouchersCounts.Manuales = periodVouchers.Count(c => !c.EsAutomatico);

        // Recent Vouchers
        response.RecentVouchers = periodVouchers
            .Where(c => c.EstadoComprobanteId == 2)
            .OrderByDescending(c => c.FechaContable)
            .Take(5)
            .Select(c => new RecentVoucherDto
            {
                NroComprobante = c.NroComprobante,
                Fecha = c.FechaContable.ToString("dd/MM/yyyy"),
                Tipo = c.TipoComprobanteDescripcion,
                Concepto = c.Concepto,
                TotalDebe = c.TotalDebe,
                TotalHaber = c.TotalHaber,
                Estado = c.EstadoComprobanteDescripcion
            }).ToList();

        // Aggregation Queries
        // For Assets (Activos, Tipo == 1), we sum all time up to the current period in the current year
        // We calculate Balance as Total Debe - Total Haber for Activos (Saldo Normal Deudor)
        var totalActivos = await _context.ComprobanteDetalles
            .Include(d => d.PlanCuenta)
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante.EmpresaId == empresaId 
                        && d.Comprobante.EstadoComprobanteId == 2
                        && d.PlanCuenta.TipoCuentaId == 1
                        && d.Comprobante.Gestion <= gestion
                        && (d.Comprobante.Gestion < gestion || d.Comprobante.PeriodoMes <= periodoMes))
            .SumAsync(d => d.Debe - d.Haber);
            
        response.StatsSummary.TotalActivos = totalActivos;
        
        // Ventas Acumuladas = Ingresos (Tipo 4) during the current gestion up to this month
        var ventasAcumuladas = await _context.ComprobanteDetalles
            .Include(d => d.PlanCuenta)
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante.EmpresaId == empresaId 
                        && d.Comprobante.EstadoComprobanteId == 2
                        && d.PlanCuenta.TipoCuentaId == 4
                        && d.Comprobante.Gestion == gestion
                        && d.Comprobante.PeriodoMes <= periodoMes)
            .SumAsync(d => d.Haber - d.Debe); // Ingresos are Acreedora
            
        response.StatsSummary.VentasAcumuladas = ventasAcumuladas;
        response.StatsSummary.VentasMeta = 600000; // Hardcoded goal for mockup

        // Costos (Tipo 5 y 6)
        var costosAcumulados = await _context.ComprobanteDetalles
            .Include(d => d.PlanCuenta)
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante.EmpresaId == empresaId 
                        && d.Comprobante.EstadoComprobanteId == 2
                        && (d.PlanCuenta.TipoCuentaId == 5 || d.PlanCuenta.TipoCuentaId == 6)
                        && d.Comprobante.Gestion == gestion
                        && d.Comprobante.PeriodoMes <= periodoMes)
            .SumAsync(d => d.Debe - d.Haber); // Gastos/Costos are Deudora

        response.ProfitSummary.Ingresos = ventasAcumuladas;
        response.ProfitSummary.Costos = costosAcumulados;
        response.ProfitSummary.UtilidadNeta = ventasAcumuladas - costosAcumulados;
        if (ventasAcumuladas > 0)
        {
            response.ProfitSummary.MargenNetoPercent = Math.Round((response.ProfitSummary.UtilidadNeta / ventasAcumuladas) * 100, 2);
        }

        // CxP Pendientes: Tipo 2 (Pasivos)
        var cxpPendientes = await _context.ComprobanteDetalles
            .Include(d => d.PlanCuenta)
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante.EmpresaId == empresaId 
                        && d.Comprobante.EstadoComprobanteId == 2
                        && d.PlanCuenta.TipoCuentaId == 2
                        && d.Comprobante.Gestion <= gestion
                        && (d.Comprobante.Gestion < gestion || d.Comprobante.PeriodoMes <= periodoMes))
            .SumAsync(d => d.Haber - d.Debe);
            
        response.StatsSummary.CxPPendientes = cxpPendientes;
        
        // IVA por liquidar - Using a mock calculation specific to IVA Debit for demonstration
        var ivaPorLiquidar = await _context.ComprobanteDetalles
            .Include(d => d.PlanCuenta)
            .Include(d => d.Comprobante)
            .Where(d => d.Comprobante.EmpresaId == empresaId 
                        && d.Comprobante.EstadoComprobanteId == 2
                        && d.PlanCuenta.Nombre.Contains("Debito Fiscal IVA")
                        && d.Comprobante.Gestion == gestion
                        && d.Comprobante.PeriodoMes == periodoMes)
            .SumAsync(d => d.Haber - d.Debe);
            
        response.StatsSummary.IvaPorLiquidar = ivaPorLiquidar > 0 ? ivaPorLiquidar : 0;
        response.StatsSummary.IvaVencimiento = "15 del siguiente mes";

        return response;
    }
}
