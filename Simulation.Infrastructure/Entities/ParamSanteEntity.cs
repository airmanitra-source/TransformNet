namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du module dépenses de santé.</summary>
public class ParamSanteEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double TauxOccupationHopitaux { get; set; } = 0.68;
    public double CoutConsultationBase { get; set; } = 8_000;
    public double CoutHospitalisationBase { get; set; } = 45_000;
    public double PartFormelleDepenseSante { get; set; } = 0.70;
    public double ProbabiliteHospitalisationBase { get; set; } = 0.10;

    public DateTime MisAJourAt { get; set; }
}
