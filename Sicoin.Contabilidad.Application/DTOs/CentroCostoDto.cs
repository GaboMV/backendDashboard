using System;
using System.Text.Json.Serialization;

namespace Sicoin.Contabilidad.Application.DTOs;

public class CentroCostoDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("codigo")]
    public string Codigo { get; set; } = null!;
    
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = null!;
    
    [JsonPropertyName("tipoId")]
    public int TipoId { get; set; }
    
    [JsonPropertyName("tipoNombre")]
    public string TipoNombre => TipoId switch
    {
        1 => "Área",
        2 => "Proyecto",
        3 => "Línea Producto",
        4 => "Región",
        _ => "Otro"
    };

    [JsonPropertyName("centroPadreId")]
    public int? CentroPadreId { get; set; }
    
    [JsonPropertyName("centroPadreCodigo")]
    public string? CentroPadreCodigo { get; set; }
    
    [JsonPropertyName("centroPadreNombre")]
    public string? CentroPadreNombre { get; set; }
    
    [JsonPropertyName("responsable")]
    public string? Responsable { get; set; }
    
    [JsonPropertyName("presupuestoAnual")]
    public decimal PresupuestoAnual { get; set; }
    
    [JsonPropertyName("gastoAcumulado")]
    public decimal GastoAcumulado { get; set; }
    
    [JsonPropertyName("totalMovimientos")]
    public int TotalMovimientos { get; set; }
    
    [JsonPropertyName("activo")]
    public bool Activo { get; set; }
}
