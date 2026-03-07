namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du secteur bancaire (agrégats monétaires, taux, réserves).</summary>
public class ParamBanqueEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double TauxInteretDepots { get; set; } = 0.045;
    public double TauxInteretCredits { get; set; } = 0.16;
    public double TauxReserveObligatoire { get; set; } = 0.13;
    public double PartDepotsAVue { get; set; } = 0.55;
    public double PartMonnaieCirculationDansM1 { get; set; } = 0.45;
    public double RatioM3SurM2 { get; set; } = 1.10;
    public double CroissanceCreditJour { get; set; } = 0.00041;
    public double PartCreditEntreprises { get; set; } = 0.75;
    public double ProbabiliteDefautCreditJour { get; set; } = 0.0003;
    public double TauxRecouvrementNPLJour { get; set; } = 0.002;
    public double AvoirsExterieursNetsInitiaux { get; set; } = 11_250_000_000_000;
    public double CreancesNettesEtatInitiales { get; set; } = 3_000_000_000_000;
    public double SCBInitial { get; set; } = 2_430_000_000_000;
    public double IntensiteInterventionBFM { get; set; } = 0.50;
    public double RatioExcedentSCBCible { get; set; } = 0.055;

    public DateTime MisAJourAt { get; set; }
}
