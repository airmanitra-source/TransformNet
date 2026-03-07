namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres globaux des entreprises (répartition sectorielle, marge, productivité).</summary>
public class ParamEntreprisesEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double PartEntreprisesAgricoles { get; set; } = 0.30;
    public double PartEntreprisesConstruction { get; set; } = 0.05;
    public double PartEntreprisesHotellerieTourisme { get; set; } = 0.03;
    public double MargeBeneficiaireEntreprise { get; set; } = 0.20;
    public double ProductiviteParEmployeJourDefaut { get; set; } = 15_000;
    public double PartB2B { get; set; } = 0.30;
    public double FacteurProductiviteInformelMin { get; set; } = 0.30;
    public double FacteurProductiviteInformelMax { get; set; } = 0.60;
    public int SeuilJoursStressTresorerie { get; set; } = 30;
    public int SeuilJoursDemandeExcedentaire { get; set; } = 15;
    public double SalaireMoyenMensuelDefaut { get; set; } = 200_000;
    public double MargeReventeImport { get; set; } = 0.25;
    public double PartExporteurProduction { get; set; } = 0.70;

    public DateTime MisAJourAt { get; set; }
}
