using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Sicoin.Contabilidad.Infrastructure.Persistence;

public class ContabilidadDbContext : DbContext, IContabilidadDbContext
{
    public ContabilidadDbContext(DbContextOptions<ContabilidadDbContext> options) : base(options)
    {
    }

    public DbSet<PlanCuenta> PlanesCuentas { get; set; }
    public DbSet<Comprobante> Comprobantes { get; set; }
    public DbSet<ComprobanteDetalle> ComprobanteDetalles { get; set; }
    public DbSet<Gestion> Gestiones { get; set; }
    public DbSet<Periodo> Periodos { get; set; }
    public DbSet<ParametroContable> ParametrosContables { get; set; }
    public DbSet<CentroCosto> CentrosCosto { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<PlanCuenta>(entity =>
        {
            entity.HasKey(e => e.PlanId);
            
            entity.HasMany(e => e.PlanesCuentasHijos)
                .WithOne()
                .HasForeignKey(e => e.PlanPadreId);

            entity.HasOne(e => e.CuentaAjuste)
                .WithMany()
                .HasForeignKey(e => e.CuentaAjusteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Gestion>(entity =>
        {
            entity.HasKey(e => e.GestionId);
            
            entity.HasMany(e => e.Periodos)
                .WithOne(p => p.Gestion)
                .HasForeignKey(p => p.GestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Periodo>(entity =>
        {
            entity.HasKey(e => e.PeriodoId);
        });

        modelBuilder.Entity<Comprobante>(entity =>
        {
            entity.HasKey(e => e.ComprobanteId);
            
            entity.HasMany(e => e.Detalles)
                .WithOne(d => d.Comprobante)
                .HasForeignKey(d => d.ComprobanteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComprobanteDetalle>(entity =>
        {
            entity.HasKey(e => e.ComprobanteDetalleId);

            entity.HasOne(d => d.PlanCuenta)
                .WithMany()
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ParametroContable>(entity =>
        {
            entity.HasKey(e => e.ParametroId);
        });

        modelBuilder.Entity<CentroCosto>(entity =>
        {
            entity.HasKey(e => e.CentroCostoId);
            
            entity.HasOne(e => e.CentroPadre)
                .WithMany(e => e.CentrosHijos)
                .HasForeignKey(e => e.CentroPadreId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
