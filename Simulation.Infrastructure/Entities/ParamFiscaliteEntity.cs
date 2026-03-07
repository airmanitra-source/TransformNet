namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres de fiscalité d'un scénario.</summary>
public class ParamFiscaliteEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double TauxIS { get; set; } = 0.20;
    public double TauxTVA { get; set; } = 0.20;
    public double TauxDirecteur { get; set; } = 0.09;
    public double TauxDroitsDouane { get; set; } = 0.12;
    public double TauxAccise { get; set; } = 0.10;
    public double TauxTaxeExport { get; set; } = 0.03;
    public double TauxCotisationsPatronalesCNaPS { get; set; } = 0.18;
    public double TauxCotisationsSalarialesCNaPS { get; set; } = 0.01;
    public double IRSAMinimumPerception { get; set; } = 2_000;

    public DateTime MisAJourAt { get; set; }
}
