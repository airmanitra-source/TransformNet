namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du marché immobilier et du logement.</summary>
public class ParamImmobilierEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double LoyerImputeJourParMenage { get; set; } = 1_000;
    public double TauxMenagesProprietaires { get; set; } = 0.65;
    public double LoyerJourLocataire { get; set; } = 3_500;
    public double ProbabiliteConstructionMaisonLocataire { get; set; } = 0.08;
    public int DureeConstructionMaisonJours { get; set; } = 240;
    public double BudgetConstructionMaisonJour { get; set; } = 7_500;
    public double PartBudgetConstructionBTP { get; set; } = 0.55;
    public double PartBudgetConstructionQuincaillerie { get; set; } = 0.30;
    public double PartBudgetConstructionTransportInformel { get; set; } = 0.15;

    public DateTime MisAJourAt { get; set; }
}
