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

    // ─── Paramètres de l'agent AgentJirama ──────────────────────────
    /// <summary>Trésorerie initiale de l'agent Jirama (MGA).</summary>
    public double TresorerieInitiale { get; set; } = 10_000_000;
    /// <summary>Nombre d'employés de base de la Jirama avant facteur d'échelle.</summary>
    public int NombreEmployesBase { get; set; } = 5_000;
    /// <summary>Salaire moyen mensuel des employés Jirama (MGA).</summary>
    public double SalaireMoyenMensuelEmploye { get; set; } = 350_000;

    public DateTime MisAJourAt { get; set; }
}
