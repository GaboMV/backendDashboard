using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sicoin.Contabilidad.Application.DTOs;

public class PeriodoDto
{
    [JsonPropertyName("id")]
    public int PeriodoId { get; set; }
    
    [JsonPropertyName("gestionId")]
    public int GestionId => Gestion?.GestionId ?? 0;

    [JsonPropertyName("mes")]
    public int Mes { get; set; }

    [JsonPropertyName("nombreMes")]
    public string NombreMes => new DateTime(2000, Mes, 1).ToString("MMMM");

    [JsonPropertyName("codigoPeriodo")]
    public string NombreCompleto => $"{Gestion?.Anio}-{Mes:D2}";
    
    [JsonPropertyName("fechaApertura")]
    public DateTime FechaInicio { get; set; }
    
    public DateTime FechaFin { get; set; }
    
    [JsonPropertyName("estado")]
    public string EstadoPeriodo { get; set; }
    
    [JsonPropertyName("esActiva")]
    public bool EsActiva => EstadoPeriodo == "Abierto" || EstadoPeriodo == "Activo";
    
    [JsonPropertyName("fechaCierre")]
    public DateTime? FechaCierreReal { get; set; }
    
    public long? UsuarioCierreId { get; set; }
    
    [JsonPropertyName("cerradoPor")]
    public string UsuarioCierreNombre { get; set; }
    
    [JsonPropertyName("totalComprobantes")]
    public int ComprobantesCount { get; set; }
    
    [JsonPropertyName("comprobantesEnBorrador")]
    public int BorradoresCount { get; set; }
    
    public bool IvaLiquidado { get; set; } 
    public bool ItLiquidado { get; set; } 
    
    [JsonIgnore]
    public GestionDto Gestion { get; set; }
}

public class GestionDto
{
    [JsonPropertyName("id")]
    public int GestionId { get; set; }
    
    public Guid EmpresaId { get; set; }
    
    [JsonPropertyName("anio")]
    public int Anio { get; set; }
    
    public string EstadoGestion { get; set; }
    
    [JsonPropertyName("esActiva")]
    public bool EsActiva => EstadoGestion == "Abierto" || EstadoGestion == "Activo";

    [JsonPropertyName("monedaBase")]
    public string MonedaBase { get; set; } = "BOB";
    
    [JsonPropertyName("fechaInicio")]
    public DateTime FechaInicio => new DateTime(Anio, 1, 1);
    
    [JsonPropertyName("fechaFin")]
    public DateTime FechaFin => new DateTime(Anio, 12, 31);
    
    [JsonPropertyName("periodos")]
    public List<PeriodoDto> Periodos { get; set; } = new();
}
