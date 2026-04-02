using Microsoft.EntityFrameworkCore;
using Sicoin.Clasificadores.Domain.Entities;

namespace Sicoin.Clasificadores.Infrastructure.Persistence;

public class ClasificadoresDbContext : DbContext
{
    public ClasificadoresDbContext(DbContextOptions<ClasificadoresDbContext> options)
        : base(options)
    {
    }

    public DbSet<TipoCuenta> TiposCuentas { get; set; }
    public DbSet<TipoComprobante> TiposComprobantes { get; set; }
    public DbSet<EstadoPeriodo> EstadosPeriodos { get; set; }
    public DbSet<Moneda> Monedas { get; set; }
    public DbSet<TipoCentroCosto> TiposCentrosCostos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Primary Keys if necessary
        modelBuilder.Entity<TipoCuenta>().HasKey(e => e.TipoCuentaId);
        modelBuilder.Entity<TipoComprobante>().HasKey(e => e.TipoComprobanteId);
        modelBuilder.Entity<EstadoPeriodo>().HasKey(e => e.EstadoPeriodoId);
        modelBuilder.Entity<Moneda>().HasKey(e => e.MonedaId);
        modelBuilder.Entity<TipoCentroCosto>().HasKey(e => e.TipoCentroCostoId);
    }
}
