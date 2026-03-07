namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres de la distribution salariale log-normale.</summary>
public class ParamDistributionSalarialeEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double SalaireMedian { get; set; } = 170_000;
    public double Sigma { get; set; } = 0.85;
    public double SalairePlancher { get; set; } = 50_000;
    public double SalairePlafond { get; set; } = 10_000_000;
    public double PartSecteurInformel { get; set; } = 0.85;

    public DateTime MisAJourAt { get; set; }
}
