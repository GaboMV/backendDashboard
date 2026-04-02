using System.Collections.Generic;

namespace Sicoin.Contabilidad.Application.DTOs;

public class DashboardResponseDto
{
    public DashboardSummaryDto StatsSummary { get; set; } = new DashboardSummaryDto();
    public ProfitSummaryDto ProfitSummary { get; set; } = new ProfitSummaryDto();
    public VouchersSummaryDto VouchersCounts { get; set; } = new VouchersSummaryDto();
    public List<RecentVoucherDto> RecentVouchers { get; set; } = new List<RecentVoucherDto>();
}

public class DashboardSummaryDto
{
    public decimal TotalActivos { get; set; }
    public decimal TotalActivosVarPercent { get; set; }
    public decimal VentasAcumuladas { get; set; }
    public decimal VentasMeta { get; set; }
    public decimal CxPPendientes { get; set; }
    public decimal CxPVencidas { get; set; }
    public decimal IvaPorLiquidar { get; set; }
    public string IvaVencimiento { get; set; } = string.Empty;
}

public class ProfitSummaryDto
{
    public decimal Ingresos { get; set; }
    public decimal Costos { get; set; }
    public decimal UtilidadNeta { get; set; }
    public decimal MargenNetoPercent { get; set; }
}

public class VouchersSummaryDto
{
    public int Confirmados { get; set; }
    public int EnBorrador { get; set; }
    public int Anulados { get; set; }
    public int Automaticos { get; set; }
    public int Manuales { get; set; }
}

public class RecentVoucherDto
{
    public string NroComprobante { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public string Estado { get; set; } = string.Empty;
}
