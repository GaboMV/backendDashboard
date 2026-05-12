using Sicoin.Contabilidad.Application.DTOs;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Application.Interfaces;

public interface IComprobanteService
{
    Task<ComprobanteSimpleResponseDto> CrearComprobanteAsync(ComprobanteRequestDto request);
    Task<bool> AnularComprobanteAsync(long comprobanteId, string motivo);
}
