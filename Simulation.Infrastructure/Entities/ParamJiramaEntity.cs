namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres de la JIRAMA (électricité + eau).</summary>
public class ParamJiramaEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double PrixElectriciteArKWh { get; set; } = 653;
    public double ConsommationElecMenageKWhJour { get; set; } = 1.03;
    public double ConsommationElecParEmployeKWhJour { get; set; } = 2.5;
    public double ConsommationElecEtatKWhJour { get; set; } = 44_400;
    public double PartProductionHydraulique { get; set; } = 0.516;
    public double TauxPertesDistribution { get; set; } = 0.289;
    public double PartConsommationElecMenages { get; set; } = 0.474;
    public double TarifEauJourMenage { get; set; } = 500;
    public double TauxAccesEau { get; set; } = 0.25;
    public double TauxAccesElectricite { get; set; } = 0.30;

    public DateTime MisAJourAt { get; set; }
}
