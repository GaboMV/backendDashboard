using Microsoft.EntityFrameworkCore;
using Sicoin.Contabilidad.Application.Interfaces;
using Sicoin.Contabilidad.Domain.Entities;
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
    }
}
