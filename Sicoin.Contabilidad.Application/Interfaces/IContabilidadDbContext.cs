using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Application.Interfaces;

public interface IContabilidadDbContext
{
    DbSet<PlanCuenta> PlanesCuentas { get; }
    DbSet<Comprobante> Comprobantes { get; }
    DbSet<ComprobanteDetalle> ComprobanteDetalles { get; }
    DbSet<Gestion> Gestiones { get; }
    DbSet<Periodo> Periodos { get; }
    DbSet<ParametroContable> ParametrosContables { get; }
    DbSet<CentroCosto> CentrosCosto { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
