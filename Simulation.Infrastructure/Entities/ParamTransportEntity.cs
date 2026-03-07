namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du module transport (parts formel/informel, coûts).</summary>
public class ParamTransportEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double PartInformelTransportPublic { get; set; } = 0.70;
    public double PartFormelCarburant { get; set; } = 0.60;
    public double PartInformelEntretien { get; set; } = 0.90;
    public double CoutTaxiBe { get; set; } = 600;
    public double EntretienVoitureJour { get; set; } = 500;
    public double EntretienFractionRevenuVoiture { get; set; } = 0.15;
    public double ConsoMotoLitrePour100km { get; set; } = 3.0;
    public double ConsoVoitureLitrePour100km { get; set; } = 8.0;
    public double CoutTransportPaiementJirama { get; set; } = 1_200;

    public DateTime MisAJourAt { get; set; }
}
