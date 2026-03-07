namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du modèle d'inflation hybride (Phillips + cost-push + monétaire).</summary>
public class ParamInflationEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double TauxInflationInitial { get; set; } = 0.08;
    public bool InflationEndogeneActivee { get; set; } = true;
    public double NAIRU { get; set; } = 0.175;
    public double CoefficientPhillips { get; set; } = 0.25;
    public double ElasticiteCarburantInflation { get; set; } = 0.15;
    public double ElasticiteImportInflation { get; set; } = 0.25;
    public double ElasticiteChangeInflation { get; set; } = 0.30;
    public double ElasticiteSalairesInflation { get; set; } = 0.10;
    public double CoefficientMonetaire { get; set; } = 0.20;
    public double PoidsAnticipationsAdaptatives { get; set; } = 0.70;
    public double PoidsAncrageInflation { get; set; } = 0.30;

    public DateTime MisAJourAt { get; set; }
}
