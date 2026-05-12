using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.DTOs;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Application.Services;

public class ComprobanteService : IComprobanteService
{
    private readonly IContabilidadDbContext _db;

    public ComprobanteService(IContabilidadDbContext db)
    {
        _db = db;
    }

    public async Task<ComprobanteSimpleResponseDto> CrearComprobanteAsync(ComprobanteRequestDto request)
    {
        // 1. Validar que el periodo esté abierto
        var periodo = await _db.Periodos
            .FirstOrDefaultAsync(p => p.Gestion.Anio == request.Fecha.Year && p.Mes == request.Fecha.Month);
        
        if (periodo == null || periodo.EstadoPeriodo != "ABIERTO") 
            throw new Exception("El periodo contable no existe o no está abierto.");

        // 2. Generar correlativo
        string prefijo = request.TipoComprobanteId switch
        {
            1 => "IN", // Ingreso
            2 => "EG", // Egreso
            3 => "TR", // Traspaso
            4 => "COM", // Compra
            5 => "VEN", // Venta
            _ => "CP"
        };

        var ultimoNro = await _db.Comprobantes
            .Where(c => c.Gestion == request.Fecha.Year && c.PeriodoMes == request.Fecha.Month && c.TipoComprobanteId == request.TipoComprobanteId)
            .OrderByDescending(c => c.NroComprobante)
            .Select(c => c.NroComprobante)
            .FirstOrDefaultAsync();

        int correlativo = 1;
        if (!string.IsNullOrEmpty(ultimoNro) && ultimoNro.Contains("-"))
        {
            if (int.TryParse(ultimoNro.Split('-').Last(), out int ultimo))
                correlativo = ultimo + 1;
        }

        string nroDocumento = $"{prefijo}-{request.Fecha.Month:D2}-{correlativo:D4}";

        // 3. Crear Entidad
        var comprobante = new Comprobante
        {
            EmpresaId = request.EmpresaId,
            SucursalId = request.SucursalId,
            TipoComprobanteId = request.TipoComprobanteId,
            TipoComprobanteDescripcion = prefijo,
            NroComprobante = nroDocumento,
            FechaContable = request.Fecha,
            Gestion = request.Fecha.Year,
            PeriodoMes = request.Fecha.Month,
            Concepto = request.Concepto,
            MonedaId = request.MonedaId,
            MonedaDescripcion = request.MonedaId == 1 ? "BOB" : "USD",
            TipoCambio = request.TipoCambio,
            ReferenciaExterna = request.ReferenciaExterna,
            TotalDebe = request.Detalles.Sum(d => d.Debe),
            TotalHaber = request.Detalles.Sum(d => d.Haber),
            EstadoComprobanteId = 2, // 2: Confirmado por defecto
            EstadoComprobanteDescripcion = "CONFIRMADO",
            UsuarioRegistroId = 1
        };

        foreach (var d in request.Detalles)
        {
            comprobante.Detalles.Add(new ComprobanteDetalle
            {
                CuentaId = d.CuentaId,
                CentroCostoId = d.CentroCostoId,
                Glosa = d.Glosa,
                Debe = d.Debe,
                Haber = d.Haber,
                AuxiliarReferencia = d.AuxiliarReferencia
            });
        }

        _db.Comprobantes.Add(comprobante);
        await _db.SaveChangesAsync();

        return new ComprobanteSimpleResponseDto
        {
            ComprobanteId = comprobante.ComprobanteId,
            NroComprobante = comprobante.NroComprobante,
            Estado = comprobante.EstadoComprobanteDescripcion
        };
    }

    public async Task<bool> AnularComprobanteAsync(long comprobanteId, string motivo)
    {
        var comp = await _db.Comprobantes.FindAsync(comprobanteId);
        if (comp == null) return false;

        comp.EstadoComprobanteId = 3; // 3: Anulado
        comp.EstadoComprobanteDescripcion = "ANULADO";
        comp.MotivoAnulacion = motivo;

        await _db.SaveChangesAsync();
        return true;
    }
}
