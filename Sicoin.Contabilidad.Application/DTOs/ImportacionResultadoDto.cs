using System.Collections.Generic;

namespace Sicoin.Contabilidad.Application.DTOs;

public class ImportacionResultadoDto
{
    public int TotalFilasLeidas { get; set; }
    public int Creadas { get; set; }
    public int Actualizadas { get; set; }
    public int Omitidas { get; set; }
    public int Errores { get; set; }
    
    public List<ImportacionDetalleDto> Detalles { get; set; } = new();
}

public class ImportacionDetalleDto
{
    public int Fila { get; set; }
    public string Cuenta { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    // "SUCCESS", "INFO", "WARNING", "ERROR"
    public string Tipo { get; set; } = string.Empty; 
}
