using System;
using System.Collections.Generic;

namespace Sicoin.Contabilidad.Application.DTOs;

// ─────────────────────────────────────────────
// LIBRO DIARIO
// ─────────────────────────────────────────────
public class LibroDiarioResponseDto
{
    public int Gestion { get; set; }
    public int MesDesde { get; set; }
    public int MesHasta { get; set; }
    public List<ComprobanteDiarioDto> Comprobantes { get; set; } = new();
    public decimal TotalDebeGeneral { get; set; }
    public decimal TotalHaberGeneral { get; set; }
}

public class ComprobanteDiarioDto
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public string? ReferenciaExterna { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool EsAutomatico { get; set; }
    public List<LineaComprobanteDiarioDto> Lineas { get; set; } = new();
}

public class LineaComprobanteDiarioDto
{
    public int NroLinea { get; set; }
    public string CodigoCuenta { get; set; } = string.Empty;
    public string NombreCuenta { get; set; } = string.Empty;
    public string? CentroCosto { get; set; }
    public string? Glosa { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}

// ─────────────────────────────────────────────
// LIBRO MAYOR
// ─────────────────────────────────────────────
public class LibroMayorResponseDto
{
    public int Gestion { get; set; }
    public int MesDesde { get; set; }
    public int MesHasta { get; set; }
    public List<CuentaMayorDto> Cuentas { get; set; } = new();
}

public class CuentaMayorDto
{
    public long CuentaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty; // "Deudora" | "Acreedora"
    public decimal SaldoInicial { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public decimal SaldoFinal { get; set; }
    public List<MovimientoMayorDto> Movimientos { get; set; } = new();
}

public class MovimientoMayorDto
{
    public DateTime Fecha { get; set; }
    public string NumeroComprobante { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public string? Glosa { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public decimal SaldoAcumulado { get; set; }
}

// ─────────────────────────────────────────────
// BALANCE DE COMPROBACIÓN
// ─────────────────────────────────────────────
public class BalanceComprobacionResponseDto
{
    public int Gestion { get; set; }
    public int MesDesde { get; set; }
    public int MesHasta { get; set; }
    public List<FilaBalanceDto> Filas { get; set; } = new();
    public decimal TotalDeudorInicial { get; set; }
    public decimal TotalAcreedorInicial { get; set; }
    public decimal TotalMovimientosDebe { get; set; }
    public decimal TotalMovimientosHaber { get; set; }
    public decimal TotalDeudorFinal { get; set; }
    public decimal TotalAcreedorFinal { get; set; }
    public bool Cuadrado { get; set; }
    public decimal Diferencia { get; set; }
}

public class FilaBalanceDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public decimal SaldoDeudorInicial { get; set; }
    public decimal SaldoAcreedorInicial { get; set; }
    public decimal MovimientosDebe { get; set; }
    public decimal MovimientosHaber { get; set; }
    public decimal SaldoDeudorFinal { get; set; }
    public decimal SaldoAcreedorFinal { get; set; }
}
