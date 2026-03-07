namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du module de taux de change dynamique MGA/USD.</summary>
public class ParamTauxChangeEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public bool TauxChangeDynamiqueActive { get; set; } = true;
    public double TauxChangeMGAParUSD { get; set; } = 4_500;
    public double ReservesBCMUSD { get; set; } = 2_500_000_000;
    public double ElasticiteChangeBalanceCommerciale { get; set; } = 0.50;
    public double PoidsChangePPA { get; set; } = 0.30;
    public double IntensiteInterventionBCM { get; set; } = 0.50;
    public double ReservesMinimalesMoisImports { get; set; } = 3.0;
    public double DepreciationStructurelleAnnuelle { get; set; } = 0.05;
    public double InflationEtrangere { get; set; } = 0.03;
    public double ElasticiteRemittancesChange { get; set; } = 0.50;

    public DateTime MisAJourAt { get; set; }
}
